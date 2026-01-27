using MediatR;
using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Queries.Users;

public class GetUsersQuery : IRequest<List<UserDto>>
{
}
