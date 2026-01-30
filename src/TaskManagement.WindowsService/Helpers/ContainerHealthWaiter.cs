using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TaskManagement.WindowsService.Helpers;

/// <summary>
/// Waits for a Docker container to report a healthy status
/// </summary>
public static class ContainerHealthWaiter
{
    public static void WaitForHealthy(ILogger logger, string containerName, int healthWaitSeconds)
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
