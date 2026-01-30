using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManagement.Application.Commands.Tasks;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;
using DomainTask = TaskManagement.Domain.Entities.Task;
using Xunit;

namespace TaskManagement.Tests.Handlers;

public class UpdateTaskCommandHandlerTests : IDisposable
{
    private readonly TaskManagementDbContext _context;

    public UpdateTaskCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<TaskManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TaskManagementDbContext(options);
        SeedDatabase();
    }

    private void SeedDatabase()
    {
        var user1 = new User
        {
            Id = 1,
            FullName = "Test User 1",
            Email = "test1@example.com",
            Telephone = "123-456-7890",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var user2 = new User
        {
            Id = 2,
            FullName = "Test User 2",
            Email = "test2@example.com",
            Telephone = "098-765-4321",
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
            Title = "Original Task",
            Description = "Original Description",
            DueDate = DateTime.Today.AddDays(1),
            Priority = Priority.Low,
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

        task.TaskTags.Add(new TaskTag
        {
            TaskId = 1,
            TagId = 1
        });

        _context.Users.AddRange(user1, user2);
        _context.Tags.Add(tag);
        _context.Tasks.Add(task);
        _context.SaveChanges();
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ValidCommand_ShouldUpdateTask()
    {
        var handler = new UpdateTaskCommandHandler(_context, NullLogger<UpdateTaskCommandHandler>.Instance);
        var command = new UpdateTaskCommand
        {
            Id = 1,
            Task = new UpdateTaskDto
            {
                Title = "Updated Task",
                Description = "Updated Description",
                DueDate = DateTime.Today.AddDays(2),
                Priority = Priority.High,
                UserIds = new List<int> { 1, 2 },
                TagIds = new List<int> { 1 }
            }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Task");
        result.Description.Should().Be("Updated Description");
        result.Priority.Should().Be(Priority.High);
        result.Users.Should().HaveCount(2);

        var taskInDb = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == 1);
        taskInDb.Should().NotBeNull();
        taskInDb!.Title.Should().Be("Updated Task");
        taskInDb.UpdatedAt.Should().BeAfter(taskInDb.CreatedAt);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_TaskNotFound_ShouldThrowException()
    {
        var handler = new UpdateTaskCommandHandler(_context, NullLogger<UpdateTaskCommandHandler>.Instance);
        var command = new UpdateTaskCommand
        {
            Id = 999,
            Task = new UpdateTaskDto
            {
                Title = "Non-existent Task",
                Description = "Description",
                DueDate = DateTime.Today.AddDays(1),
                Priority = Priority.Medium,
                UserIds = new List<int> { 1 },
                TagIds = new List<int>()
            }
        };

        await Assert.ThrowsAsync<TaskManagement.Application.Exceptions.EntityNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
