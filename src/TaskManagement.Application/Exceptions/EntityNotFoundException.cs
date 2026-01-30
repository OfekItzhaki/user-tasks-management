namespace TaskManagement.Application.Exceptions;

/// <summary>
/// Thrown when a requested entity is not found (e.g. task, user, tag by ID).
/// Maps to HTTP 404 in API.
/// </summary>
public class EntityNotFoundException : Exception
{
    public string? EntityName { get; }
    public object? EntityId { get; }

    public EntityNotFoundException(string message) : base(message) { }

    public EntityNotFoundException(string message, Exception inner) : base(message, inner) { }

    public EntityNotFoundException(string entityName, object entityId)
        : base($"{entityName} with ID {entityId} not found.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    public EntityNotFoundException(string entityName, object entityId, string message)
        : base(message)
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}
