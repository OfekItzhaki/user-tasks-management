using TaskManagement.Domain.Enums;

namespace TaskManagement.Domain.Entities;

public class Task
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public Priority Priority { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Optimistic concurrency control
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation properties
    public ICollection<UserTask> UserTasks { get; set; } = new List<UserTask>();
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
}
