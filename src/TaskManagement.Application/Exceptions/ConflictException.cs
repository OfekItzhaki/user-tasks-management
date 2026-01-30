namespace TaskManagement.Application.Exceptions;

/// <summary>
/// Thrown when an operation conflicts with the current state (e.g. concurrency, duplicate).
/// Maps to HTTP 409 in API.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }

    public ConflictException(string message, Exception inner) : base(message, inner) { }
}
