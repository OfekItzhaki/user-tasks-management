using System.Diagnostics;
using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Infrastructure;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.RabbitMQ;
using TaskManagement.WindowsService.Services;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Configure console logging with better formatting
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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<TaskManagementDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

builder.Services.AddSingleton<IRabbitMQService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RabbitMQService>>();
    var hostName = builder.Configuration["RabbitMQ:HostName"] ?? "localhost";
    return new RabbitMQService(logger, hostName);
});

builder.Services.AddHostedService<TaskReminderService>();

var host = builder.Build();

// Auto-start RabbitMQ if not running
await EnsureRabbitMQIsRunning(host.Services.GetRequiredService<ILogger<Program>>());

host.Run();

static async System.Threading.Tasks.Task EnsureRabbitMQIsRunning(ILogger logger)
{
    // Check if RabbitMQ is already running
    if (IsRabbitMQRunning())
    {
        logger.LogInformation("RabbitMQ is already running");
        return;
    }

    logger.LogInformation("RabbitMQ is not running. Attempting to start it...");

    try
    {
        // Find docker-compose.yml file (look in common locations relative to executable)
        var currentDir = AppContext.BaseDirectory;
        var projectRoot = currentDir;
        
        // Try to find docker-compose.yml by going up directories
        for (int i = 0; i < 5; i++)
        {
            var dockerComposePath = Path.Combine(projectRoot, "docker", "docker-compose.yml");
            if (File.Exists(dockerComposePath))
            {
                var dockerComposeDir = Path.GetDirectoryName(dockerComposePath);
                var projectRootDir = Path.GetDirectoryName(dockerComposeDir);
                
                logger.LogInformation("Found docker-compose.yml at: {Path}", dockerComposePath);
                
                // Start RabbitMQ using docker compose
                var startInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = $"compose -f \"{dockerComposePath}\" --project-directory \"{projectRootDir}\" up -d rabbitmq",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    
                    if (process.ExitCode == 0)
                    {
                        logger.LogInformation("RabbitMQ container started successfully. Waiting for it to be ready...");
                        
                        // Wait for RabbitMQ to be ready (up to 30 seconds)
                        var maxWait = 30;
                        var waited = 0;
                        while (!IsRabbitMQRunning() && waited < maxWait)
                        {
                            await System.Threading.Tasks.Task.Delay(2000);
                            waited += 2;
                        }
                        
                        if (IsRabbitMQRunning())
                        {
                            logger.LogInformation("RabbitMQ is now ready!");
                        }
                        else
                        {
                            logger.LogWarning("RabbitMQ container started but is not responding yet. It may still be initializing.");
                        }
                    }
                    else
                    {
                        var error = await process.StandardError.ReadToEndAsync();
                        logger.LogWarning("Failed to start RabbitMQ container. Exit code: {ExitCode}. Error: {Error}", process.ExitCode, error);
                    }
                }
                return;
            }
            
            // Go up one directory
            var parent = Directory.GetParent(projectRoot);
            if (parent == null) break;
            projectRoot = parent.FullName;
        }
        
        logger.LogWarning("Could not find docker-compose.yml file. RabbitMQ will not be started automatically.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Error attempting to start RabbitMQ. Service will continue but reminders won't be processed until RabbitMQ is available.");
    }
}

static bool IsRabbitMQRunning()
{
    try
    {
        using var tcpClient = new System.Net.Sockets.TcpClient();
        var result = tcpClient.BeginConnect("localhost", 5672, null, null);
        var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
        if (success)
        {
            tcpClient.EndConnect(result);
            return true;
        }
        return false;
    }
    catch
    {
        return false;
    }
}
