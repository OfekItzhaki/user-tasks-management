namespace TaskManagement.API.Middleware;

/// <summary>
/// Ensures a correlation ID is present for distributed tracing.
/// Uses X-Correlation-ID from request or generates one.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? Guid.NewGuid().ToString("N");
        context.Response.Headers["X-Correlation-ID"] = correlationId;
        context.Items["CorrelationId"] = correlationId;
        context.TraceIdentifier = correlationId;
        await _next(context);
    }
}
