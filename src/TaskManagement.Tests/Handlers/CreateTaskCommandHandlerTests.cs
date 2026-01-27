using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Commands.Tasks;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;
using DomainTask = TaskManagement.Domain.Entities.Task;
using Xunit;

namespace TaskManagement.Tests.Handlers;

public class CreateTaskCommandHandlerTests : IDisposable
{
    private readonly TaskManagementDbContext _context;

    public CreateTaskCommandHandlerTests()
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

        _context.Users.Add(user);
        _context.Tags.Add(tag);
        _context.SaveChanges();
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ValidCommand_ShouldCreateTask()
    {
        // Arrange
        var handler = new CreateTaskCommandHandler(_context);
        var command = new CreateTaskCommand
        {
            Task = new CreateTaskDto
            {
                Title = "New Task",
                Description = "Task Description",
                DueDate = DateTime.Today.AddDays(1),
                Priority = Priority.High,
                CreatedByUserId = 1,
                UserIds = new List<int> { 1 },
                TagIds = new List<int> { 1 }
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New Task");
        result.Priority.Should().Be(Priority.High);
        result.Tags.Should().HaveCount(1);

        var taskInDb = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == result.Id);
        taskInDb.Should().NotBeNull();
        taskInDb!.Title.Should().Be("New Task");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
