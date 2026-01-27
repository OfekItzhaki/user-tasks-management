using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs;

public class UserTaskDto
{
    public UserDto User { get; set; } = null!;
    public UserTaskRole Role { get; set; }
    public DateTime AssignedAt { get; set; }
}
