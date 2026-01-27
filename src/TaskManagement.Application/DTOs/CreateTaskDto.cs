using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs;

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public Priority Priority { get; set; }
    public int CreatedByUserId { get; set; }
    public List<int> UserIds { get; set; } = new();
    public List<int> TagIds { get; set; } = new();
}
