using MediatR;
using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Queries.Tasks;

public class GetTasksQuery : IRequest<List<TaskDto>>
{
}
