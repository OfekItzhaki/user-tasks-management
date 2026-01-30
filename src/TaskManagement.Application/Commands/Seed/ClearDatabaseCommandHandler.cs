using MediatR;
using Microsoft.Extensions.Logging;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Commands.Seed;

public class ClearDatabaseCommandHandler : IRequestHandler<ClearDatabaseCommand, ClearDatabaseResult>
{
    private readonly TaskManagementDbContext _context;
    private readonly ILogger<ClearDatabaseCommandHandler> _logger;

    public ClearDatabaseCommandHandler(TaskManagementDbContext context, ILogger<ClearDatabaseCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ClearDatabaseResult> Handle(ClearDatabaseCommand request, CancellationToken cancellationToken)
    {
        _context.TaskTags.RemoveRange(_context.TaskTags);
        _context.UserTasks.RemoveRange(_context.UserTasks);
        _context.Tasks.RemoveRange(_context.Tasks);
        _context.Tags.RemoveRange(_context.Tags);
        _context.Users.RemoveRange(_context.Users);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Database cleared successfully");

        return new ClearDatabaseResult
        {
            Success = true,
            Message = "Database cleared successfully"
        };
    }
}
