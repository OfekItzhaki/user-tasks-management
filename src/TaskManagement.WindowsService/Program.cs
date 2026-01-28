using System.Diagnostics;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Infrastructure;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.RabbitMQ;
using TaskManagement.WindowsService.Services;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Configure console logging with better formatting
var logger = LoggerFactory.Create(loggingBuilder => loggingBuilder
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

// Check if RabbitMQ is running, and try to start it if not
if (!IsRabbitMQRunning())
{
    logger.LogInformation("RabbitMQ is not running. Attempting to start it...");
    TryStartRabbitMQ(logger);
}

// Helper function to check if RabbitMQ is running
static bool IsRabbitMQRunning()
{
    try
    {
        using var tcpClient = new TcpClient();
        tcpClient.Connect("localhost", 5672);
        return true;
    }
    catch
    {
        return false;
    }
}

// Helper function to start RabbitMQ using Docker
static void TryStartRabbitMQ(ILogger logger)
{
    try
    {
        // Find project root (go up from bin/Debug/net8.0 or similar to src, then up to root)
        var currentDir = AppContext.BaseDirectory;
        var projectRoot = currentDir;
        
        // Navigate up to find docker-compose.yml
        for (int i = 0; i < 5; i++)
        {
            var dockerComposePath = Path.Combine(projectRoot, "docker", "docker-compose.yml");
            if (File.Exists(dockerComposePath))
            {
                // Found docker-compose.yml, try to start RabbitMQ
                var startInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = $"compose -f \"{dockerComposePath}\" --project-directory \"{projectRoot}\" up -d rabbitmq",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                
                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    process.WaitForExit(10000); // Wait up to 10 seconds
                    if (process.ExitCode == 0)
                    {
                        logger.LogInformation("RabbitMQ container started successfully");
                        // Give RabbitMQ a moment to initialize
                        System.Threading.Thread.Sleep(3000);
                        return;
                    }
                }
                
                // Try docker-compose (older syntax)
                startInfo = new ProcessStartInfo
                {
                    FileName = "docker-compose",
                    Arguments = $"-f \"{dockerComposePath}\" --project-directory \"{projectRoot}\" up -d rabbitmq",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                
                using var process2 = Process.Start(startInfo);
                if (process2 != null)
                {
                    process2.WaitForExit(10000);
                    if (process2.ExitCode == 0)
                    {
                        logger.LogInformation("RabbitMQ container started successfully");
                        System.Threading.Thread.Sleep(3000);
                        return;
                    }
                }
                
                logger.LogWarning("Could not start RabbitMQ container. Please start it manually: docker compose -f docker/docker-compose.yml up -d rabbitmq");
                return;
            }
            
            projectRoot = Path.GetDirectoryName(projectRoot);
            if (string.IsNullOrEmpty(projectRoot))
                break;
        }
        
        logger.LogWarning("Could not find docker-compose.yml file. RabbitMQ will not be started automatically.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Error attempting to start RabbitMQ. Please start it manually: docker compose -f docker/docker-compose.yml up -d rabbitmq");
    }
}

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

host.Run();
