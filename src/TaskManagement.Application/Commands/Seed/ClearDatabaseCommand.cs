using MediatR;

namespace TaskManagement.Application.Commands.Seed;

/// <summary>
/// Command to clear all data from the database (development only)
/// </summary>
public class ClearDatabaseCommand : IRequest<ClearDatabaseResult>
{
}

public class ClearDatabaseResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
