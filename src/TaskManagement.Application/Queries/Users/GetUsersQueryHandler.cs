using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Mappings;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Queries.Users;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly TaskManagementDbContext _context;

    public GetUsersQueryHandler(TaskManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .OrderBy(u => u.FullName)
            .ToListAsync(cancellationToken);

        return users.Select(u => u.ToUserDto()).ToList();
    }
}
