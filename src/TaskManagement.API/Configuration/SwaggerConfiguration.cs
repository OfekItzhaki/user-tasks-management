using Microsoft.OpenApi.Models;

namespace TaskManagement.API.Configuration;

/// <summary>
/// Configures Swagger/OpenAPI documentation
/// </summary>
public static class SwaggerConfiguration
{
    public static void AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Task Management API",
                Version = "v1",
                Description = "API for managing tasks, users, and tags",
                Contact = new OpenApiContact
                {
                    Name = "Task Management",
                }
            });

            // Include XML comments for better documentation
            IncludeXmlComments(c);
        });
    }

    private static void IncludeXmlComments(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
    {
        // Include API XML comments
        var apiXmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFile);
        if (File.Exists(apiXmlPath))
        {
            options.IncludeXmlComments(apiXmlPath);
        }

        // Include Application layer XML comments (for DTOs)
        var applicationAssembly = typeof(TaskManagement.Application.DTOs.CreateTaskDto).Assembly;
        var applicationXmlFile = $"{applicationAssembly.GetName().Name}.xml";
        
        // Try multiple possible paths (Debug/Release, different build configurations)
        var possiblePaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "..", "TaskManagement.Application", "bin", "Debug", "net8.0", applicationXmlFile),
            Path.Combine(AppContext.BaseDirectory, "..", "TaskManagement.Application", "bin", "Release", "net8.0", applicationXmlFile),
            Path.Combine(AppContext.BaseDirectory, applicationXmlFile)
        };

        foreach (var xmlPath in possiblePaths)
        {
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
                break;
            }
        }
    }
}
