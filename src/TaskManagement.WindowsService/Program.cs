using System.Diagnostics;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Infrastructure;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.RabbitMQ;
using TaskManagement.WindowsService.Services;
using Microsoft.Extensions.Logging;

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

// Helper function to check if SQL Server is running
static bool IsSqlServerRunning()
{
    try
    {
        using var tcpClient = new TcpClient();
        tcpClient.Connect("localhost", 1433);
        return true;
    }
    catch
    {
        return false;
    }
}

// Helper function to check if Docker is running
static bool IsDockerRunning(ILogger logger)
{
    try
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = "ps",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        
        using var process = Process.Start(startInfo);
        if (process != null)
        {
            process.WaitForExit(2000); // 2 second timeout
            return process.ExitCode == 0;
        }
        return false;
    }
    catch (Exception ex)
    {
        logger.LogDebug("Error checking Docker: {Error}", ex.Message);
        return false;
    }
}

// Helper function to start RabbitMQ using Docker
static void TryStartRabbitMQ(ILogger logger)
{
    // First check if Docker is running
    if (!IsDockerRunning(logger))
    {
        logger.LogWarning("Docker Desktop is not running. Cannot start RabbitMQ container.");
        logger.LogWarning("Please start Docker Desktop and then start RabbitMQ manually:");
        logger.LogWarning("  docker compose -f docker/docker-compose.yml up -d rabbitmq");
        return;
    }
    
    try
    {
        // Find project root - try multiple strategies
        string? projectRoot = null;
        
        // Strategy 1: Start from AppContext.BaseDirectory (executable location)
        var baseDir = AppContext.BaseDirectory;
        var currentDir = baseDir;
        
        // Navigate up from bin/Debug/net8.0 or bin/Release/net8.0
        for (int i = 0; i < 8; i++) // Increased to 8 levels to handle deeper paths
        {
            var dockerComposePath = Path.Combine(currentDir, "docker", "docker-compose.yml");
            if (File.Exists(dockerComposePath))
            {
                projectRoot = currentDir;
                break;
            }
            
            var parent = Path.GetDirectoryName(currentDir);
            if (string.IsNullOrEmpty(parent) || parent == currentDir)
                break;
            currentDir = parent;
        }
        
        // Strategy 2: If not found, try from current working directory
        if (projectRoot == null)
        {
            currentDir = Directory.GetCurrentDirectory();
            for (int i = 0; i < 8; i++)
            {
                var dockerComposePath = Path.Combine(currentDir, "docker", "docker-compose.yml");
                if (File.Exists(dockerComposePath))
                {
                    projectRoot = currentDir;
                    break;
                }
                
                var parent = Path.GetDirectoryName(currentDir);
                if (string.IsNullOrEmpty(parent) || parent == currentDir)
                    break;
                currentDir = parent;
            }
        }
        
        // Strategy 3: Try common project root patterns
        if (projectRoot == null)
        {
            var possibleRoots = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Desktop", "UserTasks"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Desktop", "try"),
            };
            
            foreach (var possibleRoot in possibleRoots)
            {
                var dockerComposePath = Path.Combine(possibleRoot, "docker", "docker-compose.yml");
                if (File.Exists(dockerComposePath))
                {
                    projectRoot = possibleRoot;
                    break;
                }
            }
        }
        
        if (projectRoot != null)
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
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();
                    process.WaitForExit(10000); // Wait up to 10 seconds
                    
                    if (process.ExitCode == 0)
                    {
                        logger.LogInformation("RabbitMQ container started successfully");
                        // Give RabbitMQ a moment to initialize
                        System.Threading.Thread.Sleep(3000);
                        return;
                    }
                    else
                    {
                        logger.LogWarning("Docker compose command failed with exit code {ExitCode}. Output: {Output}, Error: {Error}", 
                            process.ExitCode, output, error);
                    }
                }
                else
                {
                    logger.LogWarning("Failed to start docker process. Docker may not be installed or accessible.");
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
                    var output2 = process2.StandardOutput.ReadToEnd();
                    var error2 = process2.StandardError.ReadToEnd();
                    process2.WaitForExit(10000);
                    
                    if (process2.ExitCode == 0)
                    {
                        logger.LogInformation("RabbitMQ container started successfully");
                        System.Threading.Thread.Sleep(3000);
                        return;
                    }
                    else
                    {
                        logger.LogWarning("Docker-compose command failed with exit code {ExitCode}. Output: {Output}, Error: {Error}", 
                            process2.ExitCode, output2, error2);
                    }
                }
                
                logger.LogWarning("Could not start RabbitMQ container. Please start it manually: docker compose -f docker/docker-compose.yml up -d rabbitmq");
                return;
            }
        }
        
        if (projectRoot == null)
        {
            logger.LogWarning("Could not find docker-compose.yml file. Searched from: {BaseDirectory}, {WorkingDirectory}. RabbitMQ will not be started automatically.", 
                AppContext.BaseDirectory, Directory.GetCurrentDirectory());
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Error attempting to start RabbitMQ. Please start it manually: docker compose -f docker/docker-compose.yml up -d rabbitmq");
    }
}

// Create a temporary logger for RabbitMQ startup check
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

// Check if SQL Server is running (critical dependency)
if (!IsSqlServerRunning())
{
    tempLogger.LogWarning("SQL Server is not accessible on localhost:1433. The service may not function correctly.");
    tempLogger.LogWarning("Please ensure SQL Server Docker container is running: docker compose -f docker/docker-compose.yml up -d sqlserver");
    tempLogger.LogWarning("Or if using LocalDB, ensure it's started: sqllocaldb start mssqllocaldb");
}

// Check if RabbitMQ is running, and try to start it if not
if (!IsRabbitMQRunning())
{
    tempLogger.LogInformation("RabbitMQ is not running. Attempting to start it...");
    TryStartRabbitMQ(tempLogger);
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
