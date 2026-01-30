using Microsoft.AspNetCore.Cors.Infrastructure;

namespace TaskManagement.API.Configuration;

/// <summary>
/// Configures Cross-Origin Resource Sharing (CORS) policies
/// </summary>
public static class CorsConfiguration
{
    private const string PolicyName = "AllowReactApp";

    public static void AddCorsPolicy(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddCors(options =>
        {
            if (environment.IsDevelopment())
            {
                ConfigureDevelopmentCors(options);
            }
            else
            {
                ConfigureProductionCors(options, configuration);
            }
        });
    }

    public static void UseCorsPolicy(this IApplicationBuilder app)
    {
        app.UseCors(PolicyName);
    }

    private static void ConfigureDevelopmentCors(CorsOptions options)
    {
        options.AddPolicy(PolicyName, policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    }

    private static void ConfigureProductionCors(CorsOptions options, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "https://localhost:5173" };

        options.AddPolicy(PolicyName, policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
        });
    }
}
