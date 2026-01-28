using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
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
        var connectTask = tcpClient.ConnectAsync("localhost", 5672);
        if (connectTask.Wait(TimeSpan.FromSeconds(2)))
        {
            return tcpClient.Connected;
        }
        return false;
    }
    catch
    {
        return false;
    }
}

// Helper function to wait for RabbitMQ to be ready (accepting connections on port 5672)
static bool WaitForRabbitMQReady(ILogger logger, int maxWaitSeconds = 60)
{
    logger.LogInformation("Waiting for RabbitMQ to be ready on localhost:5672...");
    int waited = 0;
    int checkInterval = 2; // Check every 2 seconds
    
    while (waited < maxWaitSeconds)
    {
        if (IsRabbitMQRunning())
        {
            logger.LogInformation("RabbitMQ is ready (waited {Waited} seconds)", waited);
            return true;
        }
        
        System.Threading.Thread.Sleep(checkInterval * 1000);
        waited += checkInterval;
        
        // Log progress every 10 seconds
        if (waited % 10 == 0)
        {
            logger.LogInformation("Still waiting for RabbitMQ... ({Waited}/{MaxWaitSeconds} seconds)", waited, maxWaitSeconds);
        }
    }
    
    logger.LogWarning("RabbitMQ did not become ready within {MaxWaitSeconds} seconds", maxWaitSeconds);
    return false;
}

// Helper function to check if SQL Server is running
static bool IsSqlServerRunning()
{
    try
    {
        using var tcpClient = new TcpClient();
        var connectTask = tcpClient.ConnectAsync("localhost", 1433);
        if (connectTask.Wait(TimeSpan.FromSeconds(2)))
        {
            return tcpClient.Connected;
        }
        return false;
    }
    catch
    {
        return false;
    }
}

// Helper function to wait for SQL Server to be ready (accepting connections on port 1433)
static bool WaitForSqlServerReady(ILogger logger, int maxWaitSeconds = 60)
{
    logger.LogInformation("Waiting for SQL Server to be ready on localhost:1433...");
    int waited = 0;
    int checkInterval = 2; // Check every 2 seconds
    
    while (waited < maxWaitSeconds)
    {
        if (IsSqlServerRunning())
        {
            logger.LogInformation("SQL Server is ready (waited {Waited} seconds)", waited);
            return true;
        }
        
        System.Threading.Thread.Sleep(checkInterval * 1000);
        waited += checkInterval;
        
        // Log progress every 10 seconds
        if (waited % 10 == 0)
        {
            logger.LogInformation("Still waiting for SQL Server... ({Waited}/{MaxWaitSeconds} seconds)", waited, maxWaitSeconds);
        }
    }
    
    logger.LogWarning("SQL Server did not become ready within {MaxWaitSeconds} seconds", maxWaitSeconds);
    return false;
}

// Helper function to check if Docker is running (with timeout to prevent hanging)
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
            // Use async wait with timeout to prevent hanging when Docker Desktop is stuck
            var task = Task.Run(() => process.WaitForExit());
            if (task.Wait(TimeSpan.FromSeconds(2)))
            {
                return process.ExitCode == 0;
            }
            else
            {
                // Timeout - Docker daemon not responding (likely stuck state)
                try
                {
                    process.Kill();
                }
                catch
                {
                    // Ignore errors when killing process
                }
                return false;
            }
        }
        return false;
    }
    catch (Exception ex)
    {
        logger.LogDebug("Error checking Docker: {Error}", ex.Message);
        return false;
    }
}

// Helper function to check for and fix Docker Desktop stuck state
static bool TestAndFixDockerStuckState(ILogger logger)
{
    try
    {
        // Check if Docker Desktop process exists
        var dockerProcesses = Process.GetProcessesByName("Docker Desktop");
        bool daemonAccessible = IsDockerRunning(logger);
        
        if (dockerProcesses.Length > 0 && !daemonAccessible)
        {
            logger.LogWarning("Docker Desktop process is running but daemon is not accessible. This indicates a stuck state.");
            logger.LogInformation("Attempting to restart Docker Desktop to resolve this...");
            
            // Stop all Docker Desktop processes
            foreach (var proc in dockerProcesses)
            {
                try
                {
                    proc.Kill();
                }
                catch
                {
                    // Ignore errors
                }
            }
            
            // Wait a moment for processes to terminate
            System.Threading.Thread.Sleep(3000);
            
            // Try to start Docker Desktop
            string[] dockerPaths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Docker", "Docker", "Docker Desktop.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Docker", "Docker", "Docker Desktop.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Docker", "Docker Desktop.exe")
            };
            
            string? dockerExe = null;
            foreach (var path in dockerPaths)
            {
                if (File.Exists(path))
                {
                    dockerExe = path;
                    break;
                }
            }
            
            if (dockerExe != null)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = dockerExe,
                        UseShellExecute = true
                    });
                    logger.LogInformation("Docker Desktop restart initiated. Waiting for it to start...");
                    
                    // Wait for Docker to become ready (up to 120 seconds)
                    int maxWait = 120;
                    int waited = 0;
                    int checkInterval = 3;
                    
                    while (waited < maxWait)
                    {
                        System.Threading.Thread.Sleep(checkInterval * 1000);
                        waited += checkInterval;
                        
                        if (IsDockerRunning(logger))
                        {
                            logger.LogInformation("Docker Desktop is now ready (waited {Waited} seconds)", waited);
                            return true;
                        }
                        
                        if (waited % 15 == 0)
                        {
                            logger.LogInformation("Still waiting for Docker Desktop... ({Waited}/{MaxWait} seconds)", waited, maxWait);
                        }
                    }
                    
                    logger.LogWarning("Docker Desktop did not become ready within {MaxWait} seconds after restart", maxWait);
                    return false;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to restart Docker Desktop");
                    return false;
                }
            }
            else
            {
                logger.LogWarning("Could not find Docker Desktop executable to restart");
                return false;
            }
        }
        
        return false; // No stuck state detected
    }
    catch (Exception ex)
    {
        logger.LogDebug("Error checking Docker stuck state: {Error}", ex.Message);
        return false;
    }
}

// Helper function to start RabbitMQ using Docker
static void TryStartRabbitMQ(ILogger logger)
{
    // First check for stuck Docker Desktop state and fix it
    bool wasRestarted = TestAndFixDockerStuckState(logger);
    if (wasRestarted)
    {
        // Give Docker a moment to initialize after restart
        System.Threading.Thread.Sleep(5000);
    }
    
    // Check if Docker is running
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
                        
                        // Check Docker container health status
                        var healthCheckInfo = new ProcessStartInfo
                        {
                            FileName = "docker",
                            Arguments = "inspect --format='{{.State.Health.Status}}' taskmanagement-rabbitmq",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        };
                        
                        // Wait for container to be healthy (up to 60 seconds)
                        int healthWaitSeconds = 60;
                        int healthWaited = 0;
                        int healthCheckInterval = 2;
                        bool isHealthy = false;
                        
                        while (healthWaited < healthWaitSeconds && !isHealthy)
                        {
                            System.Threading.Thread.Sleep(healthCheckInterval * 1000);
                            healthWaited += healthCheckInterval;
                            
                            try
                            {
                                using var healthProcess = Process.Start(healthCheckInfo);
                                if (healthProcess != null)
                                {
                                    healthProcess.WaitForExit(2000);
                                    var healthStatus = healthProcess.StandardOutput.ReadToEnd().Trim();
                                    
                                    if (healthStatus == "healthy")
                                    {
                                        logger.LogInformation("RabbitMQ container is healthy");
                                        isHealthy = true;
                                        break;
                                    }
                                    else if (healthStatus == "unhealthy")
                                    {
                                        logger.LogWarning("RabbitMQ container is unhealthy. Waiting for it to recover...");
                                    }
                                    // If status is "starting" or empty, continue waiting
                                }
                            }
                            catch
                            {
                                // Health check failed, continue waiting
                            }
                            
                            if (healthWaited % 10 == 0)
                            {
                                logger.LogInformation("Waiting for RabbitMQ container to be healthy... ({HealthWaited}/{HealthWaitSeconds} seconds)", healthWaited, healthWaitSeconds);
                            }
                        }
                        
                        if (!isHealthy)
                        {
                            logger.LogWarning("RabbitMQ container did not become healthy within {HealthWaitSeconds} seconds, but will continue waiting for TCP connection", healthWaitSeconds);
                        }
                        
                        // Now wait for TCP port to be ready (this is the actual readiness check)
                        // This will be handled by WaitForRabbitMQReady() after TryStartRabbitMQ returns
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
                        logger.LogInformation("RabbitMQ container started successfully (using docker-compose)");
                        
                        // Check Docker container health status
                        var healthCheckInfo = new ProcessStartInfo
                        {
                            FileName = "docker",
                            Arguments = "inspect --format='{{.State.Health.Status}}' taskmanagement-rabbitmq",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        };
                        
                        // Wait for container to be healthy (up to 60 seconds)
                        int healthWaitSeconds = 60;
                        int healthWaited = 0;
                        int healthCheckInterval = 2;
                        bool isHealthy = false;
                        
                        while (healthWaited < healthWaitSeconds && !isHealthy)
                        {
                            System.Threading.Thread.Sleep(healthCheckInterval * 1000);
                            healthWaited += healthCheckInterval;
                            
                            try
                            {
                                using var healthProcess = Process.Start(healthCheckInfo);
                                if (healthProcess != null)
                                {
                                    healthProcess.WaitForExit(2000);
                                    var healthStatus = healthProcess.StandardOutput.ReadToEnd().Trim();
                                    
                                    if (healthStatus == "healthy")
                                    {
                                        logger.LogInformation("RabbitMQ container is healthy");
                                        isHealthy = true;
                                        break;
                                    }
                                    else if (healthStatus == "unhealthy")
                                    {
                                        logger.LogWarning("RabbitMQ container is unhealthy. Waiting for it to recover...");
                                    }
                                    // If status is "starting" or empty, continue waiting
                                }
                            }
                            catch
                            {
                                // Health check failed, continue waiting
                            }
                            
                            if (healthWaited % 10 == 0)
                            {
                                logger.LogInformation("Waiting for RabbitMQ container to be healthy... ({HealthWaited}/{HealthWaitSeconds} seconds)", healthWaited, healthWaitSeconds);
                            }
                        }
                        
                        if (!isHealthy)
                        {
                            logger.LogWarning("RabbitMQ container did not become healthy within {HealthWaitSeconds} seconds, but will continue waiting for TCP connection", healthWaitSeconds);
                        }
                        
                        // Now wait for TCP port to be ready (this will be handled by WaitForRabbitMQReady() after TryStartRabbitMQ returns)
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

// Helper function to start SQL Server using Docker
static void TryStartSqlServer(ILogger logger)
{
    // First check for stuck Docker Desktop state and fix it
    bool wasRestarted = TestAndFixDockerStuckState(logger);
    if (wasRestarted)
    {
        // Give Docker a moment to initialize after restart
        System.Threading.Thread.Sleep(5000);
    }
    
    // Check if Docker is running
    if (!IsDockerRunning(logger))
    {
        logger.LogWarning("Docker Desktop is not running. Cannot start SQL Server container.");
        logger.LogWarning("Please start Docker Desktop and then start SQL Server manually:");
        logger.LogWarning("  docker compose -f docker/docker-compose.yml up -d sqlserver");
        return;
    }
    
    try
    {
        // Find project root - try multiple strategies (same as TryStartRabbitMQ)
        string? projectRoot = null;
        
        // Strategy 1: Start from AppContext.BaseDirectory (executable location)
        var baseDir = AppContext.BaseDirectory;
        var currentDir = baseDir;
        
        // Navigate up from bin/Debug/net8.0 or bin/Release/net8.0
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
                // Found docker-compose.yml, try to start SQL Server
                var startInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = $"compose -f \"{dockerComposePath}\" --project-directory \"{projectRoot}\" up -d sqlserver",
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
                        logger.LogInformation("SQL Server container started successfully");
                        // SQL Server takes longer to initialize, so we'll wait in the main startup sequence
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
                    Arguments = $"-f \"{dockerComposePath}\" --project-directory \"{projectRoot}\" up -d sqlserver",
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
                        logger.LogInformation("SQL Server container started successfully (using docker-compose)");
                        return;
                    }
                    else
                    {
                        logger.LogWarning("Docker-compose command failed with exit code {ExitCode}. Output: {Output}, Error: {Error}", 
                            process2.ExitCode, output2, error2);
                    }
                }
                
                logger.LogWarning("Could not start SQL Server container. Please start it manually: docker compose -f docker/docker-compose.yml up -d sqlserver");
                return;
            }
        }
        
        if (projectRoot == null)
        {
            logger.LogWarning("Could not find docker-compose.yml file. Searched from: {BaseDirectory}, {WorkingDirectory}. SQL Server will not be started automatically.", 
                AppContext.BaseDirectory, Directory.GetCurrentDirectory());
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Error attempting to start SQL Server. Please start it manually: docker compose -f docker/docker-compose.yml up -d sqlserver");
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

// Check if SQL Server is running (critical dependency) and try to start it if not
if (!IsSqlServerRunning())
{
    tempLogger.LogWarning("SQL Server is not accessible on localhost:1433. Attempting to start it...");
    tempLogger.LogWarning("Please ensure SQL Server Docker container is running: docker compose -f docker/docker-compose.yml up -d sqlserver");
    tempLogger.LogWarning("Or if using LocalDB, ensure it's started: sqllocaldb start mssqllocaldb");
    
    // Try to start SQL Server container
    TryStartSqlServer(tempLogger);
    
    // Wait for SQL Server to be ready (critical - service needs database)
    // SQL Server can take 30-60 seconds to fully initialize
    if (!WaitForSqlServerReady(tempLogger, maxWaitSeconds: 90))
    {
        tempLogger.LogError("SQL Server did not become ready. Service may not function correctly.");
    }
}
else
{
    tempLogger.LogInformation("SQL Server is accessible");
}

// Check if RabbitMQ is running, and try to start it if not
if (!IsRabbitMQRunning())
{
    tempLogger.LogInformation("RabbitMQ is not running. Attempting to start it...");
    TryStartRabbitMQ(tempLogger);
    
    // Wait for RabbitMQ to be ready after starting
    if (!WaitForRabbitMQReady(tempLogger, maxWaitSeconds: 60))
    {
        tempLogger.LogWarning("RabbitMQ did not become ready. Service will continue but reminders won't be processed.");
    }
}
else
{
    tempLogger.LogInformation("RabbitMQ is accessible");
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

// Log the connection string (mask password for security) - will log after host is built

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

// Log startup information
var hostLogger = host.Services.GetRequiredService<ILogger<Program>>();
hostLogger.LogInformation("========================================");
hostLogger.LogInformation("Windows Service Starting...");
hostLogger.LogInformation("Service will check for overdue tasks every minute");
hostLogger.LogInformation("========================================");

// Log connection string (mask password)
var connectionStringForLog = connectionString;
if (connectionStringForLog.Contains("Password="))
{
    var passwordIndex = connectionStringForLog.IndexOf("Password=");
    var afterPassword = connectionStringForLog.Substring(passwordIndex + 9);
    var passwordEnd = afterPassword.IndexOf(";");
    if (passwordEnd > 0)
    {
        connectionStringForLog = connectionStringForLog.Substring(0, passwordIndex + 9) + "***" + connectionStringForLog.Substring(passwordIndex + 9 + passwordEnd);
    }
    else
    {
        connectionStringForLog = connectionStringForLog.Substring(0, passwordIndex + 9) + "***";
    }
}
hostLogger.LogInformation("Using database connection: {ConnectionString}", connectionStringForLog);
hostLogger.LogInformation("Environment: {Environment}", builder.Environment.EnvironmentName);

host.Run();
