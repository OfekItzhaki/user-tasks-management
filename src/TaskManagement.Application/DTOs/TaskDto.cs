using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs;

public class TaskDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime DueDate { get; init; }
    public Priority Priority { get; init; }
    public int CreatedByUserId { get; init; }
    public byte[] RowVersion { get; init; } = Array.Empty<byte>();
    public List<UserTaskDto> Users { get; init; } = new();
    public List<TagDto> Tags { get; init; } = new();
}
