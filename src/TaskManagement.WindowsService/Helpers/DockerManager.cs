using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TaskManagement.WindowsService.Helpers;

/// <summary>
/// Manages Docker Desktop lifecycle and health checks
/// </summary>
public static class DockerManager
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

    public static bool TestAndFixDockerStuckState(ILogger logger)
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
                Thread.Sleep(3000);

                // Try to start Docker Desktop
                string? dockerExe = FindDockerDesktopExecutable();

                if (dockerExe != null)
                {
                    return RestartDockerDesktop(logger, dockerExe);
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

    private static string? FindDockerDesktopExecutable()
    {
        string[] dockerPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Docker", "Docker", "Docker Desktop.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Docker", "Docker", "Docker Desktop.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Docker", "Docker Desktop.exe")
        };

        foreach (var path in dockerPaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }

    private static bool RestartDockerDesktop(ILogger logger, string dockerExe)
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
                Thread.Sleep(checkInterval * 1000);
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
}
