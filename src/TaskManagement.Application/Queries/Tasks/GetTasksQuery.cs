using MediatR;
using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Queries.Tasks;

public class GetTasksQuery : IRequest<PagedResult<TaskDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public int? Priority { get; set; } // Single priority (for backward compatibility)
    public List<int>? Priorities { get; set; } // Multiple priorities
    public int? UserId { get; set; }
    public List<int>? TagIds { get; set; } // Multiple tags
    public int? TagId { get; set; } // Single tag (for backward compatibility)
    public string? SortBy { get; set; } = "createdAt";
    public string? SortOrder { get; set; } = "desc";
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
