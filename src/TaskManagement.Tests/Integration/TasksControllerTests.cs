using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;
using TaskManagement.API;
using Xunit;

namespace TaskManagement.Tests.Integration;

public class TasksControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly TaskManagementDbContext _context;

    public TasksControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<TaskManagementDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<TaskManagementDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                var sp = services.BuildServiceProvider();
                _context = sp.GetRequiredService<TaskManagementDbContext>();
                SeedDatabase();
            });
        });

        _client = _factory.CreateClient();
    }

    private void SeedDatabase()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

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
    public async Task GetTasks_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateTask_ValidData_ShouldReturnCreated()
    {
        // Arrange
        var task = new CreateTaskDto
        {
            Title = "New Task",
            Description = "Task Description",
            DueDate = DateTime.Today.AddDays(1),
            Priority = Priority.Medium,
            UserId = 1,
            TagIds = new List<int> { 1 }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", task);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdTask = await response.Content.ReadFromJsonAsync<TaskDto>();
        createdTask.Should().NotBeNull();
        createdTask!.Title.Should().Be("New Task");
    }

    [Fact]
    public async Task CreateTask_InvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var task = new CreateTaskDto
        {
            Title = "", // Invalid: empty title
            Description = "Task Description",
            DueDate = DateTime.Today.AddDays(1),
            Priority = Priority.Medium,
            UserId = 1,
            TagIds = new List<int>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", task);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public void Dispose()
    {
        _context?.Database.EnsureDeleted();
        _context?.Dispose();
        _client?.Dispose();
    }
}
