using FluentValidation;
using TaskManagement.Application.Queries.Tasks;

namespace TaskManagement.Application.Validators;

public class GetTasksQueryValidator : AbstractValidator<GetTasksQuery>
{
    public GetTasksQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(1000)
            .WithMessage("Page size cannot exceed 1000.");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(200)
            .WithMessage("Search term cannot exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm));

        RuleFor(x => x.Priorities)
            .Must(priorities => priorities == null || priorities.All(p => p > 0 && p <= 4))
            .WithMessage("All priorities must be between 1 and 4.")
            .When(x => x.Priorities != null && x.Priorities.Any());

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than 0.")
            .When(x => x.UserId.HasValue);

        RuleFor(x => x.TagIds)
            .Must(tagIds => tagIds == null || tagIds.All(id => id > 0))
            .WithMessage("All tag IDs must be greater than 0.")
            .When(x => x.TagIds != null && x.TagIds.Any());

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField)
            .WithMessage("SortBy must be one of: title, duedate, priority, createdAt.")
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy));

        RuleFor(x => x.SortOrder)
            .Must(BeValidSortOrder)
            .WithMessage("SortOrder must be 'asc' or 'desc'.")
            .When(x => !string.IsNullOrWhiteSpace(x.SortOrder));
    }

    private bool BeValidSortField(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return true;

        var validFields = new[] { "title", "duedate", "priority", "createdat" };
        return validFields.Contains(sortBy.ToLower());
    }

    private bool BeValidSortOrder(string? sortOrder)
    {
        if (string.IsNullOrWhiteSpace(sortOrder))
            return true;

        var validOrders = new[] { "asc", "desc" };
        return validOrders.Contains(sortOrder.ToLower());
    }
}
