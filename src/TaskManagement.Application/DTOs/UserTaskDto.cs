using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs;

public class UserTaskDto
{
    public UserDto User { get; init; } = null!;
    public UserTaskRole Role { get; init; }
    public DateTime AssignedAt { get; init; }
}
