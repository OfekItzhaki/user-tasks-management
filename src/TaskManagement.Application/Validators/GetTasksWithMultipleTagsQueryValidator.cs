using FluentValidation;
using TaskManagement.Application.Queries.Tasks;

namespace TaskManagement.Application.Validators;

public class GetTasksWithMultipleTagsQueryValidator : AbstractValidator<GetTasksWithMultipleTagsQuery>
{
    public GetTasksWithMultipleTagsQueryValidator()
    {
        RuleFor(x => x.MinTagCount)
            .GreaterThanOrEqualTo(1)
            .WithMessage("MinTagCount must be at least 1.");
    }
}
