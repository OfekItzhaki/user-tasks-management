using MediatR;
using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Commands.Tasks;

public class UpdateTaskCommand : IRequest<TaskDto>
{
    public UpdateTaskDto Task { get; set; } = null!;
}
