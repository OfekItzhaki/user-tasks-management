using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Mappings;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Commands.Tasks;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly TaskManagementDbContext _context;

    public CreateTaskCommandHandler(TaskManagementDbContext context)
    {
        _context = context;
    }

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = new Domain.Entities.Task
        {
            Title = request.Task.Title,
            Description = request.Task.Description,
            DueDate = request.Task.DueDate,
            Priority = request.Task.Priority,
            CreatedByUserId = request.Task.CreatedByUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (request.Task.UserIds.Any())
        {
            var users = await _context.Users
                .Where(u => request.Task.UserIds.Contains(u.Id))
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;
            foreach (var user in users)
            {
                var role = user.Id == request.Task.CreatedByUserId 
                    ? UserTaskRole.Owner 
                    : UserTaskRole.Assignee;
                
                task.UserTasks.Add(new UserTask
                {
                    Task = task,
                    User = user,
                    Role = role,
                    AssignedAt = now
                });
            }
        }

        if (request.Task.TagIds.Any())
        {
            var tags = await _context.Tags
                .Where(t => request.Task.TagIds.Contains(t.Id))
                .ToListAsync(cancellationToken);

            foreach (var tag in tags)
            {
                task.TaskTags.Add(new TaskTag
                {
                    Task = task,
                    Tag = tag
                });
            }
        }

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

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
