using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using DomainTask = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Application.Commands.Seed.SeedData;

/// <summary>
/// Creates task entities and relationships for seed data
/// </summary>
public static class SeedTaskFactory
{
    public static List<DomainTask> CreateTasks(List<User> users)
    {
        return new List<DomainTask>
        {
            new DomainTask
            {
                Title = "Complete project documentation",
                Description = "Write comprehensive README and API documentation for the task management system",
                DueDate = DateTime.Today.AddDays(7),
                Priority = Priority.High,
                CreatedByUserId = users[0].Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new DomainTask
            {
                Title = "Implement user authentication",
                Description = "Add JWT-based authentication and authorization to the API",
                DueDate = DateTime.Today.AddDays(14),
                Priority = Priority.Critical,
                CreatedByUserId = users[1].Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new DomainTask
            {
                Title = "Write unit tests",
                Description = "Create comprehensive unit tests for all handlers and validators",
                DueDate = DateTime.Today.AddDays(5),
                Priority = Priority.High,
                CreatedByUserId = users[0].Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new DomainTask
            {
                Title = "Review code changes",
                Description = "Review all recent pull requests and provide feedback",
                DueDate = DateTime.Today.AddDays(3),
                Priority = Priority.Medium,
                CreatedByUserId = users[2].Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new DomainTask
            {
                Title = "Fix critical bug in task deletion",
                Description = "Investigate and fix issue where task deletion is not working correctly",
                DueDate = DateTime.Today.AddDays(1),
                Priority = Priority.Critical,
                CreatedByUserId = users[1].Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new DomainTask
            {
                Title = "Update dependencies",
                Description = "Update all NuGet packages to their latest versions",
                DueDate = DateTime.Today.AddDays(10),
                Priority = Priority.Low,
                CreatedByUserId = users[3].Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new DomainTask
            {
                Title = "Plan sprint retrospective",
                Description = "Organize and prepare materials for the upcoming sprint retrospective meeting",
                DueDate = DateTime.Today.AddDays(2),
                Priority = Priority.Medium,
                CreatedByUserId = users[4].Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
    }

    public static (List<UserTask> userTasks, List<TaskTag> taskTags) CreateRelationships(
        List<DomainTask> tasks,
        List<User> users,
        List<Tag> tags)
    {
        var userTasks = new List<UserTask>();
        var taskTags = new List<TaskTag>();
        var random = new Random(42); // Fixed seed for reproducible results

        foreach (var task in tasks)
        {
            var assignedUsers = users.OrderBy(x => random.Next()).Take(random.Next(1, 4)).ToList();
            var assignedTags = tags.OrderBy(x => random.Next()).Take(random.Next(1, 4)).ToList();

            foreach (var user in assignedUsers)
            {
                var role = user.Id == task.CreatedByUserId
                    ? UserTaskRole.Owner
                    : (random.Next(2) == 0 ? UserTaskRole.Assignee : UserTaskRole.Watcher);

                userTasks.Add(new UserTask
                {
                    UserId = user.Id,
                    TaskId = task.Id,
                    Role = role,
                    AssignedAt = DateTime.UtcNow
                });
            }

            foreach (var tag in assignedTags)
            {
                taskTags.Add(new TaskTag
                {
                    TaskId = task.Id,
                    TagId = tag.Id
                });
            }
        }

        return (userTasks, taskTags);
    }
}
