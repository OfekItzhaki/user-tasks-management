using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TaskManagement.WindowsService.Helpers;

/// <summary>
/// Detects and recovers from Docker Desktop stuck state (process running but daemon not responding)
/// </summary>
public static class DockerStuckStateHandler
{
    public static bool TestAndFixDockerStuckState(ILogger logger)
    {
        try
        {
            var dockerProcesses = Process.GetProcessesByName("Docker Desktop");
            bool daemonAccessible = DockerRunningChecker.IsDockerRunning(logger);

            if (dockerProcesses.Length == 0 || daemonAccessible)
                return false;

            logger.LogWarning("Docker Desktop process is running but daemon is not accessible. This indicates a stuck state.");
            logger.LogInformation("Attempting to restart Docker Desktop to resolve this...");

            foreach (var proc in dockerProcesses)
            {
                try { proc.Kill(); }
                catch { /* Ignore */ }
            }

            Thread.Sleep(3000);

            string? dockerExe = DockerDesktopRestarter.FindDockerDesktopExecutable();
            if (dockerExe == null)
            {
                logger.LogWarning("Could not find Docker Desktop executable to restart");
                return false;
            }

            return DockerDesktopRestarter.RestartAndWait(logger, dockerExe);
        }
        catch (Exception ex)
        {
            logger.LogDebug("Error checking Docker stuck state: {Error}", ex.Message);
            return false;
        }
    }
}
