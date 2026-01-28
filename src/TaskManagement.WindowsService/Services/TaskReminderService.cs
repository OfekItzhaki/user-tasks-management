using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.RabbitMQ;
using System.Text.Json;
using DomainTask = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.WindowsService.Services;

public class TaskReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TaskReminderService> _logger;
    private readonly IRabbitMQService _rabbitMQService;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);
    private const string ReminderQueueName = "Remainder";

    public TaskReminderService(
        IServiceProvider serviceProvider,
        ILogger<TaskReminderService> logger,
        IRabbitMQService rabbitMQService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _rabbitMQService = rabbitMQService;
    }

    protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Task Reminder Service started.");
        Console.WriteLine("[SERVICE] Task Reminder Service started");
        Console.WriteLine($"[SERVICE] Checking for overdue tasks every {_checkInterval.TotalMinutes} minute(s)");
        
        // Try to start consuming (will log warning if RabbitMQ is not available)
        _rabbitMQService.StartConsuming(ReminderQueueName, ProcessReminder);
        Console.WriteLine("[SERVICE] Attempted to subscribe to 'Remainder' queue");
        Console.WriteLine("[SERVICE] Note: If RabbitMQ is not running, reminders won't be processed.");
        Console.WriteLine("[SERVICE] Start RabbitMQ with: .\\scripts\\start-rabbitmq.ps1");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndPublishOverdueTasks(stoppingToken);
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "Database error checking overdue tasks. This may indicate a schema mismatch. Error: {Message}", sqlEx.Message);
                Console.WriteLine($"[ERROR] Database error: {sqlEx.Message}");
                Console.WriteLine("[INFO] Service will continue running. Please check database migrations.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking overdue tasks: {Message}", ex.Message);
                Console.WriteLine($"[ERROR] Error checking overdue tasks: {ex.Message}");
            }

            await System.Threading.Tasks.Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async System.Threading.Tasks.Task CheckAndPublishOverdueTasks(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();

        try
        {
            var now = DateTime.UtcNow;
            var overdueTasks = await dbContext.Tasks
                .Include(t => t.UserTasks)
                    .ThenInclude(ut => ut.User)
                .Where(t => t.DueDate <= now)
                .ToListAsync(cancellationToken);

            foreach (var task in overdueTasks)
            {
                foreach (var userTask in task.UserTasks)
                {
                    var reminderMessage = new
                    {
                        TaskId = task.Id,
                        TaskTitle = task.Title,
                        DueDate = task.DueDate,
                        UserName = userTask.User.FullName,
                        UserEmail = userTask.User.Email,
                        Role = userTask.Role.ToString()
                    };

                    var messageJson = JsonSerializer.Serialize(reminderMessage);
                    _rabbitMQService.PublishMessage(ReminderQueueName, messageJson);

                    _logger.LogInformation("Published reminder for overdue task: {TaskId} - {TaskTitle} to user: {UserName}", 
                        task.Id, task.Title, userTask.User.FullName);
                    Console.WriteLine($"[PUBLISH] Reminder published - Task: {task.Title} (ID: {task.Id}) to {userTask.User.FullName}");
                }
            }

            if (overdueTasks.Any())
            {
                _logger.LogInformation("Found and published {Count} overdue task reminders", overdueTasks.Count);
                Console.WriteLine($"[SUMMARY] Found and published {overdueTasks.Count} overdue task reminder(s)");
            }
            else
            {
                Console.WriteLine("[CHECK] No overdue tasks found");
            }
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "Database error in CheckAndPublishOverdueTasks: {Message}", sqlEx.Message);
            Console.WriteLine($"[ERROR] Database error: {sqlEx.Message}");
            throw; // Re-throw to be caught by outer handler
        }
    }

    private void ProcessReminder(string message)
    {
        try
        {
            var reminder = JsonSerializer.Deserialize<ReminderMessage>(message);
            if (reminder != null)
            {
                // Log the message in the required format
                var logMessage = $"Hi your Task is due {reminder.TaskTitle}";
                _logger.LogInformation(logMessage);
                
                // Also write to console for better visibility
                Console.WriteLine($"[REMINDER] {logMessage}");
                Console.WriteLine($"         Task ID: {reminder.TaskId}, User: {reminder.UserName}, Due: {reminder.DueDate:yyyy-MM-dd HH:mm:ss}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing reminder message: {Message}", message);
            Console.WriteLine($"[ERROR] Failed to process reminder: {ex.Message}");
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
