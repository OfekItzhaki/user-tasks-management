namespace TaskManagement.Application.DTOs;

public class UserDto
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Telephone { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
