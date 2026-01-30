using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.RabbitMQ;
using TaskManagement.WindowsService.Services;

namespace TaskManagement.WindowsService.Configuration;

/// <summary>
/// Configures the host application with services, logging, and dependencies
/// </summary>
public static class HostConfiguration
{
    public static IHost ConfigureHost(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        ConfigureLogging(builder);
        ConfigureServices(builder);

        return builder.Build();
    }

    private static void ConfigureLogging(HostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(options =>
        {
            options.FormatterName = "simple";
        });
        builder.Logging.AddSimpleConsole(options =>
        {
            options.IncludeScopes = false;
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
            options.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled;
        });
    }

    private static void ConfigureServices(HostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found. Set it via environment variable ConnectionStrings__DefaultConnection or User Secrets. See instructions/CONFIG.md.");
        if (connectionString.Contains("***SET_VIA_ENV_OR_USER_SECRETS***", StringComparison.Ordinal))
            throw new InvalidOperationException("Replace the placeholder connection string. Set ConnectionStrings:DefaultConnection via environment variable ConnectionStrings__DefaultConnection or User Secrets. See instructions/CONFIG.md.");

        builder.Services.AddDbContext<TaskManagementDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            }));

        builder.Services.Configure<TaskReminderServiceOptions>(
            builder.Configuration.GetSection(TaskReminderServiceOptions.SectionName));

        builder.Services.AddSingleton<IRabbitMQService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMQService>>();
            var hostName = builder.Configuration["RabbitMQ:HostName"] ?? "localhost";
            return new RabbitMQService(logger, hostName);
        });

        builder.Services.AddHostedService<TaskReminderService>();
    }

    public static void LogStartupInformation(IHost host, string connectionString, string environmentName)
    {
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        var options = host.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<TaskReminderServiceOptions>>().Value;
        
        logger.LogInformation("========================================");
        logger.LogInformation("Windows Service Starting...");
        logger.LogInformation("Service will check for overdue tasks every {Interval} minute(s), queue: {QueueName}", 
            options.CheckInterval.TotalMinutes, options.QueueName);
        logger.LogInformation("========================================");

        // Log connection string (mask password)
        var maskedConnectionString = MaskPassword(connectionString);
        logger.LogInformation("Using database connection: {ConnectionString}", maskedConnectionString);
        logger.LogInformation("Environment: {Environment}", environmentName);
    }

    private static string MaskPassword(string connectionString)
    {
        if (connectionString.Contains("Password="))
        {
            var passwordIndex = connectionString.IndexOf("Password=");
            var afterPassword = connectionString.Substring(passwordIndex + 9);
            var passwordEnd = afterPassword.IndexOf(";");
            if (passwordEnd > 0)
            {
                return connectionString.Substring(0, passwordIndex + 9) + "***" + connectionString.Substring(passwordIndex + 9 + passwordEnd);
            }
            else
            {
                return connectionString.Substring(0, passwordIndex + 9) + "***";
            }
        }
        return connectionString;
    }
}
