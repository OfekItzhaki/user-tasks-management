using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManagement.Application.Commands.Tasks;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;
using DomainTask = TaskManagement.Domain.Entities.Task;
using Xunit;

namespace TaskManagement.Tests.Handlers;

public class DeleteTaskCommandHandlerTests : IDisposable
{
    private readonly TaskManagementDbContext _context;

    public DeleteTaskCommandHandlerTests()
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

        var task = new DomainTask
        {
            Id = 1,
            Title = "Task to Delete",
            Description = "Description",
            DueDate = DateTime.Today.AddDays(1),
            Priority = Priority.Medium,
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

        _context.Users.Add(user);
        _context.Tasks.Add(task);
        _context.SaveChanges();
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ValidId_ShouldDeleteTask()
    {
        var handler = new DeleteTaskCommandHandler(_context, NullLogger<DeleteTaskCommandHandler>.Instance);
        var command = new DeleteTaskCommand { Id = 1 };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();

        var taskInDb = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == 1);
        taskInDb.Should().BeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_TaskNotFound_ShouldThrowException()
    {
        var handler = new DeleteTaskCommandHandler(_context, NullLogger<DeleteTaskCommandHandler>.Instance);
        var command = new DeleteTaskCommand { Id = 999 };

        await Assert.ThrowsAsync<TaskManagement.Application.Exceptions.EntityNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
