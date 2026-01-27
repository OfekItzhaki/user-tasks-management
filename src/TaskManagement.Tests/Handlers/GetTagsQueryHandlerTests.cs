using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Queries.Tags;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using DomainTask = TaskManagement.Domain.Entities.Task;
using Xunit;

namespace TaskManagement.Tests.Handlers;

public class GetTagsQueryHandlerTests : IDisposable
{
    private readonly TaskManagementDbContext _context;

    public GetTagsQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<TaskManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TaskManagementDbContext(options);
        SeedDatabase();
    }

    private void SeedDatabase()
    {
        var tags = new List<Tag>
        {
            new Tag { Id = 1, Name = "Tag 1", CreatedAt = DateTime.UtcNow },
            new Tag { Id = 2, Name = "Tag 2", Color = "#FF0000", CreatedAt = DateTime.UtcNow },
            new Tag { Id = 3, Name = "Tag 3", CreatedAt = DateTime.UtcNow }
        };

        _context.Tags.AddRange(tags);
        _context.SaveChanges();
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnAllTags()
    {
        var handler = new GetTagsQueryHandler(_context);
        var query = new GetTagsQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(t => t.Name == "Tag 1");
        result.Should().Contain(t => t.Name == "Tag 2");
        result.Should().Contain(t => t.Name == "Tag 3");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
