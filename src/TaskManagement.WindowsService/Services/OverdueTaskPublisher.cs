using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.RabbitMQ;

namespace TaskManagement.WindowsService.Services;

/// <summary>
/// Queries overdue tasks and publishes reminder messages to RabbitMQ
/// </summary>
public static class OverdueTaskPublisher
{
    public static async Task PublishOverdueRemindersAsync(
        TaskManagementDbContext dbContext,
        IRabbitMQService rabbitMQService,
        string queueName,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        logger.LogDebug("Checking for overdue tasks (current time: {Now})", now);

        var overdueTasks = await dbContext.Tasks
            .Include(t => t.UserTasks)
                .ThenInclude(ut => ut.User)
            .Where(t => t.DueDate <= now)
            .ToListAsync(cancellationToken);

        foreach (var task in overdueTasks)
        {
            foreach (var userTask in task.UserTasks)
            {
                var correlationId = Guid.NewGuid().ToString("N");
                var reminderMessage = new
                {
                    task.Id,
                    TaskTitle = task.Title,
                    task.DueDate,
                    UserName = userTask.User.FullName,
                    UserEmail = userTask.User.Email,
                    Role = userTask.Role.ToString(),
                    CorrelationId = correlationId
                };
                var messageJson = JsonSerializer.Serialize(reminderMessage);
                rabbitMQService.PublishMessage(queueName, messageJson, new Dictionary<string, string> { ["X-Correlation-ID"] = correlationId });
                logger.LogInformation("Published reminder for overdue task: {TaskId} - {TaskTitle} to user: {UserName} [CorrelationId: {CorrelationId}]",
                    task.Id, task.Title, userTask.User.FullName, correlationId);
            }
        }

        if (overdueTasks.Any())
        {
            logger.LogInformation("Found and published {Count} overdue task reminder(s)", overdueTasks.Count);
        }
        else
        {
            var totalTasks = await dbContext.Tasks.CountAsync(cancellationToken);
            logger.LogInformation("Checked for overdue tasks. No overdue tasks found (checked {TaskCount} total tasks)", totalTasks);
        }
    }
}
