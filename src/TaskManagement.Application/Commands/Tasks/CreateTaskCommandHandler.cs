using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Mappings;
using TaskManagement.Domain.Entities;
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
            UserId = request.Task.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add tags if provided
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

        // Load related data for response
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
