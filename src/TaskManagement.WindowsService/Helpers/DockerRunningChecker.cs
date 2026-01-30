using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TaskManagement.WindowsService.Helpers;

/// <summary>
/// Checks whether the Docker daemon is running and responsive
/// </summary>
public static class DockerRunningChecker
{
    public static bool IsDockerRunning(ILogger logger)
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
                var task = Task.Run(() => process.WaitForExit());
                if (task.Wait(TimeSpan.FromSeconds(2)))
                {
                    return process.ExitCode == 0;
                }
                try
                {
                    process.Kill();
                }
                catch
                {
                    // Ignore
                }
                return false;
            }
            return false;
        }
        catch (Exception ex)
        {
            logger.LogDebug("Error checking Docker: {Error}", ex.Message);
            return false;
        }
    }
}
