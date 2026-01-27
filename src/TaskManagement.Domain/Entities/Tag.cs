namespace TaskManagement.Domain.Entities;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
}
