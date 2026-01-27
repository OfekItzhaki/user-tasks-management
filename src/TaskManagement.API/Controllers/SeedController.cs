using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using DomainTask = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly TaskManagementDbContext _context;
    private readonly ILogger<SeedController> _logger;

    public SeedController(TaskManagementDbContext context, ILogger<SeedController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> SeedDatabase()
    {
        try
        {
            if (await _context.Users.AnyAsync())
            {
                return BadRequest("Database already contains data. Clear database first if you want to reseed.");
            }

            var users = new List<User>
            {
                new User
                {
                    FullName = "John Doe",
                    Email = "john.doe@example.com",
                    Telephone = "555-0101",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    FullName = "Jane Smith",
                    Email = "jane.smith@example.com",
                    Telephone = "555-0102",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    FullName = "Bob Johnson",
                    Email = "bob.johnson@example.com",
                    Telephone = "555-0103",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    FullName = "Alice Williams",
                    Email = "alice.williams@example.com",
                    Telephone = "555-0104",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    FullName = "Charlie Brown",
                    Email = "charlie.brown@example.com",
                    Telephone = "555-0105",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            var tags = new List<Tag>
            {
                new Tag { Name = "Urgent", Color = "#FF0000", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "Important", Color = "#FFA500", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "Development", Color = "#0000FF", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "Testing", Color = "#00FF00", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "Documentation", Color = "#800080", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "Meeting", Color = "#FFC0CB", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "Review", Color = "#FFFF00", CreatedAt = DateTime.UtcNow },
                new Tag { Name = "Bug Fix", Color = "#FF4500", CreatedAt = DateTime.UtcNow }
            };

            _context.Users.AddRange(users);
            _context.Tags.AddRange(tags);
            await _context.SaveChangesAsync();

            var tasks = new List<DomainTask>
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

            _context.Tasks.AddRange(tasks);
            await _context.SaveChangesAsync();

            var userTasks = new List<UserTask>();
            var taskTags = new List<TaskTag>();

            var random = new Random();

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

            _context.UserTasks.AddRange(userTasks);
            _context.TaskTags.AddRange(taskTags);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Database seeded successfully with {UserCount} users, {TagCount} tags, and {TaskCount} tasks",
                users.Count, tags.Count, tasks.Count);

            return Ok(new
            {
                message = "Database seeded successfully",
                users = users.Count,
                tags = tags.Count,
                tasks = tasks.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding database");
            return StatusCode(500, new { message = "Error seeding database", error = ex.Message });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> ClearDatabase()
    {
        try
        {
            _context.TaskTags.RemoveRange(_context.TaskTags);
            _context.UserTasks.RemoveRange(_context.UserTasks);
            _context.Tasks.RemoveRange(_context.Tasks);
            _context.Tags.RemoveRange(_context.Tags);
            _context.Users.RemoveRange(_context.Users);
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Database cleared successfully");

            return Ok(new { message = "Database cleared successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing database");
            return StatusCode(500, new { message = "Error clearing database", error = ex.Message });
        }
    }
}
