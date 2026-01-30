using Microsoft.Extensions.Logging;

namespace TaskManagement.WindowsService.Helpers;

/// <summary>
/// Starts Docker containers (RabbitMQ, SQL Server) using docker-compose
/// </summary>
public static class DockerComposeStarter
{
    public static void TryStartRabbitMQ(ILogger logger)
    {
        TryStartContainer(logger, "rabbitmq", healthCheckWaitSeconds: 60);
    }

    public static void TryStartSqlServer(ILogger logger)
    {
        TryStartContainer(logger, "sqlserver", healthCheckWaitSeconds: 0);
    }

    private static void TryStartContainer(ILogger logger, string containerName, int healthCheckWaitSeconds)
    {
        if (DockerManager.TestAndFixDockerStuckState(logger))
        {
            Thread.Sleep(5000);
        }

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

            if (DockerComposeRunner.TryDockerCompose(logger, dockerComposePath, projectRoot, containerName))
            {
                logger.LogInformation("{ContainerName} container started successfully", containerName);
                if (healthCheckWaitSeconds > 0)
                {
                    ContainerHealthWaiter.WaitForHealthy(logger, containerName, healthCheckWaitSeconds);
                }
                return;
            }

            if (DockerComposeRunner.TryDockerComposeOldSyntax(logger, dockerComposePath, projectRoot, containerName))
            {
                logger.LogInformation("{ContainerName} container started successfully (using docker-compose)", containerName);
                if (healthCheckWaitSeconds > 0)
                {
                    ContainerHealthWaiter.WaitForHealthy(logger, containerName, healthCheckWaitSeconds);
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
}
