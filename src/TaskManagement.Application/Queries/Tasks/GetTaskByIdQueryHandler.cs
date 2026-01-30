using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Mappings;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Queries.Tasks;

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto>
{
    private readonly TaskManagementDbContext _context;
    private readonly ILogger<GetTaskByIdQueryHandler> _logger;

    public GetTaskByIdQueryHandler(TaskManagementDbContext context, ILogger<GetTaskByIdQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TaskDto> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting task by ID: {TaskId}", request.Id);
        var task = await _context.Tasks
            .Include(t => t.UserTasks)
                .ThenInclude(ut => ut.User)
            .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task == null)
        {
            _logger.LogWarning("Task not found: {TaskId}", request.Id);
            throw new EntityNotFoundException("Task", request.Id, $"Task with ID {request.Id} not found.");
        }

        _logger.LogInformation("Retrieved task {TaskId}: {Title}", task.Id, task.Title);
        return task.ToTaskDto();
    }
}
