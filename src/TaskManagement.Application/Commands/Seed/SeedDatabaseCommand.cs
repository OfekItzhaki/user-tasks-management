using MediatR;

namespace TaskManagement.Application.Commands.Seed;

/// <summary>
/// Command to seed the database with initial data (development only)
/// </summary>
public class SeedDatabaseCommand : IRequest<SeedDatabaseResult>
{
}

public class SeedDatabaseResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int UsersCreated { get; set; }
    public int TagsCreated { get; set; }
    public int TasksCreated { get; set; }
}
