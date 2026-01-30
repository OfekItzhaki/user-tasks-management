using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TaskManagement.WindowsService.Helpers;

/// <summary>
/// Locates Docker Desktop executable and restarts it, waiting until the daemon is ready
/// </summary>
public static class DockerDesktopRestarter
{
    public static string? FindDockerDesktopExecutable()
    {
        var dockerPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Docker", "Docker", "Docker Desktop.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Docker", "Docker", "Docker Desktop.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Docker", "Docker Desktop.exe")
        };

        foreach (var path in dockerPaths)
        {
            if (File.Exists(path))
                return path;
        }
        return null;
    }

    public static bool RestartAndWait(ILogger logger, string dockerExe)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = dockerExe,
                UseShellExecute = true
            });
            logger.LogInformation("Docker Desktop restart initiated. Waiting for it to start...");

            const int maxWait = 120;
            const int checkInterval = 3;
            int waited = 0;

            while (waited < maxWait)
            {
                Thread.Sleep(checkInterval * 1000);
                waited += checkInterval;

                if (DockerRunningChecker.IsDockerRunning(logger))
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
