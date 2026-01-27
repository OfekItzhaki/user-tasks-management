using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Queries.Tasks;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;
using DomainTask = TaskManagement.Domain.Entities.Task;
using Xunit;

namespace TaskManagement.Tests.Handlers;

public class GetTasksQueryHandlerTests : IDisposable
{
    private readonly TaskManagementDbContext _context;

    public GetTasksQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<TaskManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TaskManagementDbContext(options);
        SeedDatabase();
    }

    private void SeedDatabase()
    {
        var user = new User
        {
            Id = 1,
            FullName = "Test User",
            Email = "test@example.com",
            Telephone = "123-456-7890",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var tag = new Tag
        {
            Id = 1,
            Name = "Test Tag",
            CreatedAt = DateTime.UtcNow
        };

        var task1 = new DomainTask
        {
            Id = 1,
            Title = "Task 1",
            Description = "Description 1",
            DueDate = DateTime.Today.AddDays(1),
            Priority = Priority.High,
            CreatedByUserId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var task2 = new DomainTask
        {
            Id = 2,
            Title = "Task 2",
            Description = "Description 2",
            DueDate = DateTime.Today.AddDays(2),
            Priority = Priority.Medium,
            CreatedByUserId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        task1.UserTasks.Add(new UserTask
        {
            UserId = 1,
            TaskId = 1,
            Role = UserTaskRole.Owner,
            AssignedAt = DateTime.UtcNow
        });

        task1.TaskTags.Add(new TaskTag { TaskId = 1, TagId = 1 });

        _context.Users.Add(user);
        _context.Tags.Add(tag);
        _context.Tasks.AddRange(task1, task2);
        _context.SaveChanges();
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnAllTasks()
    {
        var handler = new GetTasksQueryHandler(_context);
        var query = new GetTasksQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Title == "Task 1");
        result.Should().Contain(t => t.Title == "Task 2");
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldIncludeUsersAndTags()
    {
        var handler = new GetTasksQueryHandler(_context);
        var query = new GetTasksQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        var task1 = result.First(t => t.Id == 1);
        task1.Users.Should().HaveCount(1);
        task1.Tags.Should().HaveCount(1);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
