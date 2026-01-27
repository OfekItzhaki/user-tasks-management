using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Queries.Tasks;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;
using DomainTask = TaskManagement.Domain.Entities.Task;
using Xunit;

namespace TaskManagement.Tests.Handlers;

public class GetTaskByIdQueryHandlerTests : IDisposable
{
    private readonly TaskManagementDbContext _context;

    public GetTaskByIdQueryHandlerTests()
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

        var task = new DomainTask
        {
            Id = 1,
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.Today.AddDays(1),
            Priority = Priority.High,
            CreatedByUserId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        task.UserTasks.Add(new UserTask
        {
            UserId = 1,
            TaskId = 1,
            Role = UserTaskRole.Owner,
            AssignedAt = DateTime.UtcNow
        });

        task.TaskTags.Add(new TaskTag { TaskId = 1, TagId = 1 });

        _context.Users.Add(user);
        _context.Tags.Add(tag);
        _context.Tasks.Add(task);
        _context.SaveChanges();
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ValidId_ShouldReturnTask()
    {
        var handler = new GetTaskByIdQueryHandler(_context);
        var query = new GetTaskByIdQuery { Id = 1 };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Task");
        result.Users.Should().HaveCount(1);
        result.Tags.Should().HaveCount(1);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_InvalidId_ShouldThrowException()
    {
        var handler = new GetTaskByIdQueryHandler(_context);
        var query = new GetTaskByIdQuery { Id = 999 };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(query, CancellationToken.None));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
