using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.RabbitMQ;
using System.Text.Json;

namespace TaskManagement.WindowsService.Services;

public class TaskReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TaskReminderService> _logger;
    private readonly IRabbitMQService _rabbitMQService;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);
    private const string ReminderQueueName = "TaskReminders";

    public TaskReminderService(
        IServiceProvider serviceProvider,
        ILogger<TaskReminderService> logger,
        IRabbitMQService rabbitMQService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _rabbitMQService = rabbitMQService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Start consuming reminders
        _rabbitMQService.StartConsuming(ReminderQueueName, ProcessReminder);

        _logger.LogInformation("Task Reminder Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndPublishOverdueTasks(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking overdue tasks");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckAndPublishOverdueTasks(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();

        var now = DateTime.UtcNow;
        var overdueTasks = await dbContext.Tasks
            .Include(t => t.User)
            .Where(t => t.DueDate <= now)
            .ToListAsync(cancellationToken);

        foreach (var task in overdueTasks)
        {
            var reminderMessage = new
            {
                TaskId = task.Id,
                TaskTitle = task.Title,
                DueDate = task.DueDate,
                UserName = task.User.FullName,
                UserEmail = task.User.Email
            };

            var messageJson = JsonSerializer.Serialize(reminderMessage);
            _rabbitMQService.PublishMessage(ReminderQueueName, messageJson);

            _logger.LogInformation("Published reminder for overdue task: {TaskId} - {TaskTitle}", task.Id, task.Title);
        }

        if (overdueTasks.Any())
        {
            _logger.LogInformation("Found and published {Count} overdue task reminders", overdueTasks.Count);
        }
    }

    private void ProcessReminder(string message)
    {
        try
        {
            var reminder = JsonSerializer.Deserialize<ReminderMessage>(message);
            if (reminder != null)
            {
                _logger.LogInformation("Hi your Task is due {TaskTitle} (Task ID: {TaskId}, Due Date: {DueDate})",
                    reminder.TaskTitle, reminder.TaskId, reminder.DueDate);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing reminder message: {Message}", message);
        }
    }

    private class ReminderMessage
    {
        public int TaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
    }
}
