using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Exceptions;

namespace TaskManagement.API.Middleware;

/// <summary>
/// Global exception handling middleware. Catches exceptions and maps them to HTTP responses.
/// Keeps controllers thin: no exception-handling logic in controllers.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, response) = ex switch
        {
            EntityNotFoundException entityEx => (HttpStatusCode.NotFound, new { message = entityEx.Message }),
            KeyNotFoundException keyEx => (HttpStatusCode.NotFound, new { message = keyEx.Message }),
            ConflictException conflictEx => (HttpStatusCode.Conflict, new { message = conflictEx.Message }),
            ValidationException valEx => (HttpStatusCode.BadRequest, new { errors = valEx.Errors.Select(e => new { property = e.PropertyName, message = e.ErrorMessage }) }),
            DbUpdateConcurrencyException => (HttpStatusCode.Conflict, new { message = "This resource has been modified by another user. Please refresh and try again." }),
            _ => (HttpStatusCode.InternalServerError, new
            {
                message = "An error occurred while processing your request.",
                detail = _env.IsDevelopment() ? ex.Message : (string?)null,
                traceId = context.TraceIdentifier
            })
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
        else
            _logger.LogWarning(ex, "Handled exception: {Message}", ex.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
