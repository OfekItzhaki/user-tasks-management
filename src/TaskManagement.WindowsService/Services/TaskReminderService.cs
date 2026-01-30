using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.RabbitMQ;
using TaskManagement.WindowsService.Models;

namespace TaskManagement.WindowsService.Services;

public class TaskReminderServiceOptions
{
    public const string SectionName = "TaskReminder";
    public int CheckIntervalMinutes { get; set; } = 1;
    public TimeSpan CheckInterval => TimeSpan.FromMinutes(Math.Max(1, CheckIntervalMinutes));
    public string QueueName { get; set; } = "Reminder";
}

public class TaskReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TaskReminderService> _logger;
    private readonly IRabbitMQService _rabbitMQService;
    private readonly TaskReminderServiceOptions _options;

    public TaskReminderService(
        IServiceProvider serviceProvider,
        ILogger<TaskReminderService> logger,
        IRabbitMQService rabbitMQService,
        IOptions<TaskReminderServiceOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _rabbitMQService = rabbitMQService;
        _options = options.Value;
    }

    protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Task Reminder Service started. Checking for overdue tasks every {Interval} minute(s), queue: {QueueName}.",
            _options.CheckInterval.TotalMinutes, _options.QueueName);

        _rabbitMQService.StartConsuming(_options.QueueName, ProcessReminder);

        // Graceful shutdown: stop consuming when host is stopping so no new messages are accepted
        using var stopRegistration = stoppingToken.Register(() => _rabbitMQService.StopConsuming());

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndPublishOverdueTasks(stoppingToken);
                }
                catch (Microsoft.Data.SqlClient.SqlException sqlEx)
                {
                    _logger.LogError(sqlEx, "Database error checking overdue tasks. This may indicate a schema mismatch. Error: {Message}", sqlEx.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking overdue tasks: {Message}", ex.Message);
                }

                await System.Threading.Tasks.Task.Delay(_options.CheckInterval, stoppingToken);
            }
        }
        finally
        {
            _rabbitMQService.StopConsuming();
        }
    }

    private async System.Threading.Tasks.Task CheckAndPublishOverdueTasks(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();

        try
        {
            await OverdueTaskPublisher.PublishOverdueRemindersAsync(
                dbContext, _rabbitMQService, _options.QueueName, _logger, cancellationToken);
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "Database error in CheckAndPublishOverdueTasks: {Message}", sqlEx.Message);
            throw;
        }
    }

    private bool ProcessReminder(string message)
    {
        try
        {
            var reminder = JsonSerializer.Deserialize<ReminderMessage>(message);
            if (reminder != null)
            {
                var correlationId = reminder.CorrelationId ?? "(none)";
                _logger.LogInformation(
                    "Reminder processed: Task {TaskId} - {TaskTitle} for {UserName} [CorrelationId: {CorrelationId}]",
                    reminder.TaskId, reminder.TaskTitle, reminder.UserName, correlationId);
                return true;
            }
            _logger.LogWarning("Received null or invalid reminder message");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing reminder message: {Message}", message);
            return false;
        }
    }
}
