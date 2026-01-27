using MediatR;
using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Queries.Tasks;

public class GetTaskByIdQuery : IRequest<TaskDto>
{
    public int Id { get; set; }
}
