using FluentAssertions;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Validators;
using TaskManagement.Domain.Enums;
using Xunit;

namespace TaskManagement.Tests.Validators;

public class UpdateTaskDtoValidatorTests
{
    private readonly UpdateTaskDtoValidator _validator;

    public UpdateTaskDtoValidatorTests()
    {
        _validator = new UpdateTaskDtoValidator();
    }

    [Fact]
    public void Validate_ValidTask_ShouldPass()
    {
        var task = new UpdateTaskDto
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.Today.AddDays(1),
            Priority = Priority.Medium,
            UserIds = new List<int> { 1 },
            TagIds = new List<int> { 1 }
        };

        var result = _validator.Validate(task);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyTitle_ShouldFail()
    {
        var task = new UpdateTaskDto
        {
            Title = "",
            Description = "Test Description",
            DueDate = DateTime.Today.AddDays(1),
            Priority = Priority.Medium,
            UserIds = new List<int> { 1 },
            TagIds = new List<int>()
        };

        var result = _validator.Validate(task);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_EmptyUserIds_ShouldFail()
    {
        var task = new UpdateTaskDto
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.Today.AddDays(1),
            Priority = Priority.Medium,
            UserIds = new List<int>(),
            TagIds = new List<int>()
        };

        var result = _validator.Validate(task);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserIds");
    }
}
