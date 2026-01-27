using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Mappings;

public static class UserExtensions
{
    public static UserDto ToUserDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Telephone = user.Telephone,
            Email = user.Email,
        };
    }
}
