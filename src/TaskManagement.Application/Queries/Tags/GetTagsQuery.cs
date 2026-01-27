using MediatR;
using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Queries.Tags;

public class GetTagsQuery : IRequest<List<TagDto>>
{
}
