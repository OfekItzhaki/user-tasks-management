using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Mappings;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Queries.Users;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly TaskManagementDbContext _context;
    private readonly ILogger<GetUsersQueryHandler> _logger;

    public GetUsersQueryHandler(TaskManagementDbContext context, ILogger<GetUsersQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting all users");
        var users = await _context.Users
            .OrderBy(u => u.FullName)
            .ToListAsync(cancellationToken);
        _logger.LogInformation("Returned {Count} users", users.Count);
        return users.Select(u => u.ToUserDto()).ToList();
    }
}
