using MediatR;
using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Queries.Tasks;

public class GetTasksWithMultipleTagsQuery : IRequest<List<TasksWithTagsDto>>
{
    public int MinTagCount { get; set; } = 2;
}
