using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Mappings;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Queries.Tags;

public class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, List<TagDto>>
{
    private readonly TaskManagementDbContext _context;

    public GetTagsQueryHandler(TaskManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<TagDto>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await _context.Tags
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        return tags.Select(t => t.ToTagDto()).ToList();
    }
}
