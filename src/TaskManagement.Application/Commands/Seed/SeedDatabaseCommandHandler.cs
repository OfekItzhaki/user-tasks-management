using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Commands.Seed.SeedData;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Commands.Seed;

public class SeedDatabaseCommandHandler : IRequestHandler<SeedDatabaseCommand, SeedDatabaseResult>
{
    private readonly TaskManagementDbContext _context;
    private readonly ILogger<SeedDatabaseCommandHandler> _logger;

    public SeedDatabaseCommandHandler(TaskManagementDbContext context, ILogger<SeedDatabaseCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SeedDatabaseResult> Handle(SeedDatabaseCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Users.AnyAsync(cancellationToken))
        {
            return new SeedDatabaseResult
            {
                Success = false,
                Message = "Database already contains data. Clear database first if you want to reseed."
            };
        }

        // Create seed data
        var users = SeedDataFactory.CreateUsers();
        var tags = SeedDataFactory.CreateTags();

        _context.Users.AddRange(users);
        _context.Tags.AddRange(tags);
        await _context.SaveChangesAsync(cancellationToken);

        var tasks = SeedTaskFactory.CreateTasks(users);
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync(cancellationToken);

        var (userTasks, taskTags) = SeedTaskFactory.CreateRelationships(tasks, users, tags);
        _context.UserTasks.AddRange(userTasks);
        _context.TaskTags.AddRange(taskTags);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Database seeded successfully with {UserCount} users, {TagCount} tags, and {TaskCount} tasks",
            users.Count, tags.Count, tasks.Count);

        return new SeedDatabaseResult
        {
            Success = true,
            Message = "Database seeded successfully",
            UsersCreated = users.Count,
            TagsCreated = tags.Count,
            TasksCreated = tasks.Count
        };
    }
}
