using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Mappings;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Queries.Tasks;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, List<TaskDto>>
{
    private readonly TaskManagementDbContext _context;

    public GetTasksQueryHandler(TaskManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaskDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _context.Tasks
            .Include(t => t.User)
            .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return tasks.Select(t => t.ToTaskDto()).ToList();
    }
}
