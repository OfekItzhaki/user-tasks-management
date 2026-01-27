namespace TaskManagement.Domain.Entities;

public class TaskTag
{
    public int TaskId { get; set; }
    public int TagId { get; set; }

    // Navigation properties
    public Task Task { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
