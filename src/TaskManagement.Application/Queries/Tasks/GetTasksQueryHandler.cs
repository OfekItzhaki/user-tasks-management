using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Mappings;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Queries.Tasks;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, PagedResult<TaskDto>>
{
    private readonly TaskManagementDbContext _context;

    public GetTasksQueryHandler(TaskManagementDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<TaskDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tasks
            .Include(t => t.UserTasks)
                .ThenInclude(ut => ut.User)
            .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
            .AsQueryable();

        // Search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(t => 
                t.Title.ToLower().Contains(searchTerm) || 
                t.Description.ToLower().Contains(searchTerm));
        }

        // Priority filter - multiple priorities only
        if (request.Priorities != null && request.Priorities.Count > 0)
        {
            var priorityValues = request.Priorities.Select(p => (Domain.Enums.Priority)p).ToList();
            query = query.Where(t => priorityValues.Contains(t.Priority));
        }

        // User filter
        if (request.UserId.HasValue)
        {
            query = query.Where(t => t.UserTasks.Any(ut => ut.UserId == request.UserId.Value));
        }

        // Tag filter - multiple tags only
        if (request.TagIds != null && request.TagIds.Count > 0)
        {
            query = query.Where(t => t.TaskTags.Any(tt => request.TagIds.Contains(tt.TagId)));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "title" => request.SortOrder?.ToLower() == "asc" 
                ? query.OrderBy(t => t.Title) 
                : query.OrderByDescending(t => t.Title),
            "duedate" => request.SortOrder?.ToLower() == "asc" 
                ? query.OrderBy(t => t.DueDate) 
                : query.OrderByDescending(t => t.DueDate),
            "priority" => request.SortOrder?.ToLower() == "asc" 
                ? query.OrderBy(t => t.Priority) 
                : query.OrderByDescending(t => t.Priority),
            _ => request.SortOrder?.ToLower() == "asc" 
                ? query.OrderBy(t => t.CreatedAt) 
                : query.OrderByDescending(t => t.CreatedAt)
        };

        // Pagination - ensure valid values
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, request.PageSize);

        var tasks = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TaskDto>
        {
            Items = tasks.Select(t => t.ToTaskDto()).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
