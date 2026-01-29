using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TaskManagement.WindowsService.Helpers;

/// <summary>
/// Starts Docker containers using docker-compose
/// </summary>
public static class DockerComposeStarter
{
    public static void TryStartRabbitMQ(ILogger logger)
    {
        TryStartContainer(logger, "rabbitmq", 60);
    }

    public static void TryStartSqlServer(ILogger logger)
    {
        TryStartContainer(logger, "sqlserver", 0); // No health check wait for SQL Server here
    }

    private static void TryStartContainer(ILogger logger, string containerName, int healthCheckWaitSeconds)
    {
        // First check for stuck Docker Desktop state and fix it
        bool wasRestarted = DockerManager.TestAndFixDockerStuckState(logger);
        if (wasRestarted)
        {
            // Give Docker a moment to initialize after restart
            Thread.Sleep(5000);
        }

        // Check if Docker is running
        if (!DockerManager.IsDockerRunning(logger))
        {
            logger.LogWarning("Docker Desktop is not running. Cannot start {ContainerName} container.", containerName);
            logger.LogWarning("Please start Docker Desktop and then start {ContainerName} manually:", containerName);
            logger.LogWarning("  docker compose -f docker/docker-compose.yml up -d {ContainerName}", containerName);
            return;
        }

        try
        {
            string? projectRoot = ProjectRootFinder.FindProjectRoot();

            if (projectRoot == null)
            {
                logger.LogWarning("Could not find docker-compose.yml file. Searched from: {BaseDirectory}, {WorkingDirectory}. {ContainerName} will not be started automatically.",
                    AppContext.BaseDirectory, Directory.GetCurrentDirectory(), containerName);
                return;
            }

            var dockerComposePath = Path.Combine(projectRoot, "docker", "docker-compose.yml");
            if (!File.Exists(dockerComposePath))
            {
                logger.LogWarning("docker-compose.yml not found at {Path}", dockerComposePath);
                return;
            }

            // Try modern docker compose command first
            if (TryDockerCompose(logger, dockerComposePath, projectRoot, containerName))
            {
                logger.LogInformation("{ContainerName} container started successfully", containerName);
                
                if (healthCheckWaitSeconds > 0)
                {
                    WaitForContainerHealth(logger, containerName, healthCheckWaitSeconds);
                }
                return;
            }

            // Try older docker-compose command
            if (TryDockerComposeOldSyntax(logger, dockerComposePath, projectRoot, containerName))
            {
                logger.LogInformation("{ContainerName} container started successfully (using docker-compose)", containerName);
                
                if (healthCheckWaitSeconds > 0)
                {
                    WaitForContainerHealth(logger, containerName, healthCheckWaitSeconds);
                }
                return;
            }

            logger.LogWarning("Could not start {ContainerName} container. Please start it manually: docker compose -f docker/docker-compose.yml up -d {ContainerName}",
                containerName, containerName);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error attempting to start {ContainerName}. Please start it manually: docker compose -f docker/docker-compose.yml up -d {ContainerName}",
                containerName, containerName);
        }
    }

    private static bool TryDockerCompose(ILogger logger, string dockerComposePath, string projectRoot, string containerName)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"compose -f \"{dockerComposePath}\" --project-directory \"{projectRoot}\" up -d {containerName}",
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
                return true;
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

        return false;
    }

    private static bool TryDockerComposeOldSyntax(ILogger logger, string dockerComposePath, string projectRoot, string containerName)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "docker-compose",
            Arguments = $"-f \"{dockerComposePath}\" --project-directory \"{projectRoot}\" up -d {containerName}",
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
            process.WaitForExit(10000);

            if (process.ExitCode == 0)
            {
                return true;
            }
            else
            {
                logger.LogWarning("Docker-compose command failed with exit code {ExitCode}. Output: {Output}, Error: {Error}",
                    process.ExitCode, output, error);
            }
        }

        return false;
    }

    private static void WaitForContainerHealth(ILogger logger, string containerName, int healthWaitSeconds)
    {
        var healthCheckInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"inspect --format='{{{{.State.Health.Status}}}}' taskmanagement-{containerName}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        int healthWaited = 0;
        int healthCheckInterval = 2;
        bool isHealthy = false;

        while (healthWaited < healthWaitSeconds && !isHealthy)
        {
            Thread.Sleep(healthCheckInterval * 1000);
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
                        logger.LogInformation("{ContainerName} container is healthy", containerName);
                        isHealthy = true;
                        break;
                    }
                    else if (healthStatus == "unhealthy")
                    {
                        logger.LogWarning("{ContainerName} container is unhealthy. Waiting for it to recover...", containerName);
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
                logger.LogInformation("Waiting for {ContainerName} container to be healthy... ({HealthWaited}/{HealthWaitSeconds} seconds)",
                    containerName, healthWaited, healthWaitSeconds);
            }
        }

        if (!isHealthy)
        {
            logger.LogWarning("{ContainerName} container did not become healthy within {HealthWaitSeconds} seconds, but will continue waiting for TCP connection",
                containerName, healthWaitSeconds);
        }
    }
}
