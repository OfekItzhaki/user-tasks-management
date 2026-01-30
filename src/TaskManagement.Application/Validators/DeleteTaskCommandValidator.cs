using FluentValidation;
using TaskManagement.Application.Commands.Tasks;

namespace TaskManagement.Application.Validators;

public class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
{
    public DeleteTaskCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Task ID must be greater than 0.");
    }
}
