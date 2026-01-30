using FluentValidation;
using TaskManagement.Application.Queries.Tasks;

namespace TaskManagement.Application.Validators;

public class GetTaskByIdQueryValidator : AbstractValidator<GetTaskByIdQuery>
{
    public GetTaskByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Task ID must be greater than 0.");
    }
}
