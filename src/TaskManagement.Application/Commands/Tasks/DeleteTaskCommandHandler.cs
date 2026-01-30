using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Exceptions;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Commands.Tasks;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, bool>
{
    private readonly TaskManagementDbContext _context;
    private readonly ILogger<DeleteTaskCommandHandler> _logger;

    public DeleteTaskCommandHandler(TaskManagementDbContext context, ILogger<DeleteTaskCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Deleting task: {TaskId}", request.Id);
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task == null)
        {
            _logger.LogWarning("Task not found for delete: {TaskId}", request.Id);
            throw new EntityNotFoundException("Task", request.Id, $"Task with ID {request.Id} not found.");
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted task {TaskId}", request.Id);
        return true;
    }
}
