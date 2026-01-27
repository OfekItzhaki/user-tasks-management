using MediatR;
using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Commands.Tasks;

public class CreateTaskCommand : IRequest<TaskDto>
{
    public CreateTaskDto Task { get; set; } = null!;
}
