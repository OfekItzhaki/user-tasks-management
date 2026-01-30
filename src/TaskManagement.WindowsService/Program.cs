using Microsoft.Extensions.Configuration;
using TaskManagement.WindowsService.Configuration;
using TaskManagement.WindowsService.Helpers;

var tempLogger = StartupLoggerFactory.CreateProgramLogger();
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
