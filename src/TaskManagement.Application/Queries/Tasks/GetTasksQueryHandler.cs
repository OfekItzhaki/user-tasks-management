using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Mappings;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Queries.Tasks;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, PagedResult<TaskDto>>
{
    private readonly TaskManagementDbContext _context;
    private readonly ILogger<GetTasksQueryHandler> _logger;

    public GetTasksQueryHandler(TaskManagementDbContext context, ILogger<GetTasksQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<TaskDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting tasks: Page={Page}, PageSize={PageSize}, SearchTerm={SearchTerm}", request.Page, request.PageSize, request.SearchTerm ?? "(none)");
        // Sanitize inputs at the application layer
        var sanitizedSearchTerm = Common.InputSanitizer.Sanitize(request.SearchTerm);
        var sanitizedSortBy = string.IsNullOrWhiteSpace(request.SortBy) 
            ? "createdAt" 
            : Common.InputSanitizer.Sanitize(request.SortBy);
        var sanitizedSortOrder = string.IsNullOrWhiteSpace(request.SortOrder) 
            ? "desc" 
            : Common.InputSanitizer.Sanitize(request.SortOrder).ToLower();

        var query = _context.Tasks
            .Include(t => t.UserTasks)
                .ThenInclude(ut => ut.User)
            .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(sanitizedSearchTerm))
        {
            var searchTerm = sanitizedSearchTerm.ToLower();
            query = query.Where(t => 
                t.Title.ToLower().Contains(searchTerm) || 
                t.Description.ToLower().Contains(searchTerm));
        }

        if (request.Priorities != null && request.Priorities.Count > 0)
        {
            var priorityValues = request.Priorities.Select(p => (Domain.Enums.Priority)p).ToList();
            query = query.Where(t => priorityValues.Contains(t.Priority));
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(t => t.UserTasks.Any(ut => ut.UserId == request.UserId.Value));
        }

        if (request.TagIds != null && request.TagIds.Count > 0)
        {
            var tagIdsList = request.TagIds.ToList();
            query = query.Where(t => t.TaskTags.Count(tt => tagIdsList.Contains(tt.TagId)) == tagIdsList.Count);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        query = sanitizedSortBy.ToLower() switch
        {
            "title" => sanitizedSortOrder == "asc" 
                ? query.OrderBy(t => t.Title) 
                : query.OrderByDescending(t => t.Title),
            "duedate" => sanitizedSortOrder == "asc" 
                ? query.OrderBy(t => t.DueDate) 
                : query.OrderByDescending(t => t.DueDate),
            "priority" => sanitizedSortOrder == "asc" 
                ? query.OrderBy(t => t.Priority) 
                : query.OrderByDescending(t => t.Priority),
            _ => sanitizedSortOrder == "asc" 
                ? query.OrderBy(t => t.CreatedAt) 
                : query.OrderByDescending(t => t.CreatedAt)
        };

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, request.PageSize);

        var tasks = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Returned {Count} tasks (page {Page}, total {TotalCount})", tasks.Count, page, totalCount);
        return new PagedResult<TaskDto>
        {
            Items = tasks.Select(t => t.ToTaskDto()).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
