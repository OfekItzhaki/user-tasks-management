using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs;

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public Priority Priority { get; set; }
    public UserDto User { get; set; } = null!;
    public List<TagDto> Tags { get; set; } = new();
}
