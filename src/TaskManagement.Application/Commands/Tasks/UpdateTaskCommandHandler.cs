using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Mappings;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Commands.Tasks;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto>
{
    private readonly TaskManagementDbContext _context;

    public UpdateTaskCommandHandler(TaskManagementDbContext context)
    {
        _context = context;
    }

    public async Task<TaskDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks
            .Include(t => t.User)
            .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
            .FirstOrDefaultAsync(t => t.Id == request.Task.Id, cancellationToken);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {request.Task.Id} not found.");
        }

        task.Title = request.Task.Title;
        task.Description = request.Task.Description;
        task.DueDate = request.Task.DueDate;
        task.Priority = request.Task.Priority;
        task.UserId = request.Task.UserId;
        task.UpdatedAt = DateTime.UtcNow;

        // Update tags
        _context.TaskTags.RemoveRange(task.TaskTags);
        
        if (request.Task.TagIds.Any())
        {
            var tags = await _context.Tags
                .Where(t => request.Task.TagIds.Contains(t.Id))
                .ToListAsync(cancellationToken);

            foreach (var tag in tags)
            {
                task.TaskTags.Add(new Domain.Entities.TaskTag
                {
                    Task = task,
                    Tag = tag
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Reload to ensure all navigation properties are loaded
        await _context.Entry(task)
            .Reference(t => t.User)
            .LoadAsync(cancellationToken);

        await _context.Entry(task)
            .Collection(t => t.TaskTags)
            .Query()
            .Include(tt => tt.Tag)
            .LoadAsync(cancellationToken);

        return task.ToTaskDto();
    }
}
