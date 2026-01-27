using FluentAssertions;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Validators;
using TaskManagement.Domain.Enums;
using Xunit;

namespace TaskManagement.Tests.Validators;

public class CreateTaskDtoValidatorTests
{
    private readonly CreateTaskDtoValidator _validator;

    public CreateTaskDtoValidatorTests()
    {
        _validator = new CreateTaskDtoValidator();
    }

    [Fact]
    public void Validate_ValidTask_ShouldPass()
    {
        // Arrange
        var task = new CreateTaskDto
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.Today.AddDays(1),
            Priority = Priority.Medium,
            CreatedByUserId = 1,
            UserIds = new List<int> { 1, 2 },
            TagIds = new List<int> { 1, 2 }
        };

        // Act
        var result = _validator.Validate(task);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyTitle_ShouldFail()
    {
        // Arrange
        var task = new CreateTaskDto
        {
            Title = "",
            Description = "Test Description",
            DueDate = DateTime.Today.AddDays(1),
            Priority = Priority.Medium,
            CreatedByUserId = 1,
            UserIds = new List<int> { 1 },
            TagIds = new List<int>()
        };

        // Act
        var result = _validator.Validate(task);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_TitleTooLong_ShouldFail()
    {
        // Arrange
        var task = new CreateTaskDto
        {
            Title = new string('A', 201),
            Description = "Test Description",
            DueDate = DateTime.Today.AddDays(1),
            Priority = Priority.Medium,
            CreatedByUserId = 1,
            UserIds = new List<int> { 1 },
            TagIds = new List<int>()
        };

        // Act
        var result = _validator.Validate(task);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_PastDueDate_ShouldFail()
    {
        // Arrange
        var task = new CreateTaskDto
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.Today.AddDays(-1),
            Priority = Priority.Medium,
            CreatedByUserId = 1,
            UserIds = new List<int> { 1 },
            TagIds = new List<int>()
        };

        // Act
        var result = _validator.Validate(task);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DueDate");
    }

    [Fact]
    public void Validate_InvalidCreatedByUserId_ShouldFail()
    {
        var task = new CreateTaskDto
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.Today.AddDays(1),
            Priority = Priority.Medium,
            CreatedByUserId = 0,
            UserIds = new List<int> { 1 },
            TagIds = new List<int>()
        };

        var result = _validator.Validate(task);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CreatedByUserId");
    }

    [Fact]
    public void Validate_EmptyUserIds_ShouldFail()
    {
        var task = new CreateTaskDto
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.Today.AddDays(1),
            Priority = Priority.Medium,
            CreatedByUserId = 1,
            UserIds = new List<int>(),
            TagIds = new List<int>()
        };

        var result = _validator.Validate(task);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserIds");
    }
}
