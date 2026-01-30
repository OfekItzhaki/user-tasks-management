using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Mappings;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Queries.Tags;

public class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, List<TagDto>>
{
    private readonly TaskManagementDbContext _context;
    private readonly ILogger<GetTagsQueryHandler> _logger;

    public GetTagsQueryHandler(TaskManagementDbContext context, ILogger<GetTagsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<TagDto>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting all tags");
        var tags = await _context.Tags
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
        _logger.LogInformation("Returned {Count} tags", tags.Count);
        return tags.Select(t => t.ToTagDto()).ToList();
    }
}
