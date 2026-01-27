using TaskManagement.Domain.Enums;

namespace TaskManagement.Domain.Entities;

public class UserTask
{
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public UserTaskRole Role { get; set; } = UserTaskRole.Assignee;
    public DateTime AssignedAt { get; set; }

    // Navigation properties
    public Task Task { get; set; } = null!;
    public User User { get; set; } = null!;
}
