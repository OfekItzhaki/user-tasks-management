using System.Diagnostics;
using System.Text;

namespace TaskManagement.API.Middleware;

/// <summary>
/// Logs request and response for debugging and audit.
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = (string?)context.Items["CorrelationId"] ?? context.TraceIdentifier;
        var sw = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "";
        var query = context.Request.QueryString.Value ?? "";

        _logger.LogInformation("Request {Method} {Path}{Query} [CorrelationId: {CorrelationId}]",
            method, path, query, correlationId);

        await _next(context);

        sw.Stop();
        _logger.LogInformation("Response {StatusCode} for {Method} {Path} in {ElapsedMs}ms [CorrelationId: {CorrelationId}]",
            context.Response.StatusCode, method, path, sw.ElapsedMilliseconds, correlationId);
    }
}
