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
            .Include(t => t.UserTasks)
                .ThenInclude(ut => ut.User)
            .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {request.Id} not found.");
        }

        // Optimistic concurrency check - set original value for EF Core to compare
        if (request.Task.RowVersion != null && request.Task.RowVersion.Length > 0)
        {
            _context.Entry(task).Property(t => t.RowVersion).OriginalValue = request.Task.RowVersion;
        }

        task.Title = request.Task.Title;
        task.Description = request.Task.Description;
        task.DueDate = request.Task.DueDate;
        task.Priority = request.Task.Priority;
        task.UpdatedAt = DateTime.UtcNow;

        _context.UserTasks.RemoveRange(task.UserTasks);
        
        if (request.Task.UserIds.Any())
        {
            var users = await _context.Users
                .Where(u => request.Task.UserIds.Contains(u.Id))
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;
            foreach (var user in users)
            {
                var role = user.Id == task.CreatedByUserId 
                    ? Domain.Enums.UserTaskRole.Owner 
                    : Domain.Enums.UserTaskRole.Assignee;
                
                task.UserTasks.Add(new Domain.Entities.UserTask
                {
                    Task = task,
                    User = user,
                    Role = role,
                    AssignedAt = now
                });
            }
        }

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

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Reload the task to get the current RowVersion
            await _context.Entry(task).ReloadAsync(cancellationToken);
            throw new DbUpdateConcurrencyException(
                $"Task with ID {request.Id} has been modified by another user. Please refresh and try again.", ex);
        }

        await _context.Entry(task)
            .Collection(t => t.UserTasks)
            .Query()
            .Include(ut => ut.User)
            .LoadAsync(cancellationToken);

        await _context.Entry(task)
            .Collection(t => t.TaskTags)
            .Query()
            .Include(tt => tt.Tag)
            .LoadAsync(cancellationToken);

        return task.ToTaskDto();
    }
}
