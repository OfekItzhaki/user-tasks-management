using MediatR;

namespace TaskManagement.Application.Commands.Tasks;

public class DeleteTaskCommand : IRequest<bool>
{
    public int Id { get; set; }
}
