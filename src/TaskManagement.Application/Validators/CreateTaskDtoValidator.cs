using FluentValidation;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Validators;

public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Due date is required.")
            .Must(BeTodayOrFuture).WithMessage("Due date must be today or in the future.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Priority must be a valid value.");

        RuleFor(x => x.CreatedByUserId)
            .GreaterThan(0).WithMessage("Created by user ID must be greater than 0.");

        RuleFor(x => x.UserIds)
            .NotNull().WithMessage("User IDs cannot be null.")
            .Must(ids => ids != null && ids.Count > 0).WithMessage("At least one user must be assigned to the task.");

        RuleFor(x => x.TagIds)
            .NotNull().WithMessage("Tag IDs cannot be null.")
            .Must(ids => ids != null && ids.Count > 0).WithMessage("At least one tag must be selected.");
    }

    private bool BeTodayOrFuture(DateTime date)
    {
        return date.Date >= DateTime.Today;
    }
}
