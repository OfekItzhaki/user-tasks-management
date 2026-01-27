using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Entities;
using DomainTask = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Application.Mappings;

public static class TaskExtensions
{
    public static TaskDto ToTaskDto(this DomainTask task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            Priority = task.Priority,
            CreatedByUserId = task.CreatedByUserId,
            Users = task.UserTasks.Select(ut => new UserTaskDto
            {
                User = new UserDto
                {
                    Id = ut.User.Id,
                    FullName = ut.User.FullName,
                    Telephone = ut.User.Telephone,
                    Email = ut.User.Email
                },
                Role = ut.Role,
                AssignedAt = ut.AssignedAt
            }).ToList(),
            Tags = task.TaskTags.Select(tt => new TagDto
            {
                Id = tt.Tag.Id,
                Name = tt.Tag.Name,
                Color = tt.Tag.Color
            }).ToList()
        };
    }

    public static TagDto ToTagDto(this Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color
        };
    }
}
