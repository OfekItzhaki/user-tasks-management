using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaskManagement.WindowsService.Configuration;
using TaskManagement.WindowsService.Helpers;

// Create a temporary logger for startup dependency checks
var tempLogger = LoggerFactory.Create(loggingBuilder => loggingBuilder
    .AddConsole(options =>
    {
        options.FormatterName = "simple";
    })
    .AddSimpleConsole(options =>
    {
        options.IncludeScopes = false;
        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
        options.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled;
    })).CreateLogger("Program");

// Check and start required dependencies (SQL Server, RabbitMQ)
StartupDependencyChecker.EnsureDependenciesRunning(tempLogger);

// Configure and build the host
var host = HostConfiguration.ConfigureHost(args);

// Get connection string from configuration for logging purposes
var configuration = host.Services.GetRequiredService<IConfiguration>();
var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var environmentName = host.Services.GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>().EnvironmentName;

// Log startup information
HostConfiguration.LogStartupInformation(host, connectionString, environmentName);

host.Run();
