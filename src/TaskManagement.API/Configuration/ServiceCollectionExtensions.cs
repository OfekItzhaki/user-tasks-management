using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TaskManagement.API.Configuration;

/// <summary>
/// Centralizes service registration for the API
/// </summary>
public static class ServiceCollectionExtensions
{
    public static void AddTaskManagementApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers(options =>
        {
            options.ModelBinderProviders.Insert(0, new Microsoft.AspNetCore.Mvc.ModelBinding.Binders.ArrayModelBinderProvider());
        });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerDocumentation();

        // API versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });
        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = false; // Version via query ?api-version=1.0 or header to avoid breaking existing clients
        });

        // Rate limiting (built-in .NET 8)
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 2
                    }));
            options.OnRejected = async (context, _) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                await context.HttpContext.Response.WriteAsJsonAsync(new { message = "Too many requests. Please try again later." });
            };
        });

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddHealthChecks()
            .AddSqlServer(connectionString ?? string.Empty, name: "sqlserver", failureStatus: HealthStatus.Unhealthy);
    }
}
