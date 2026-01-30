using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TaskManagement.WindowsService.Helpers;

/// <summary>
/// Runs docker compose / docker-compose commands to start a single service
/// </summary>
public static class DockerComposeRunner
{
    private const int ProcessWaitMs = 10000;

    public static bool TryDockerCompose(ILogger logger, string dockerComposePath, string projectRoot, string containerName)
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

        return RunComposeProcess(logger, startInfo, containerName, "Docker compose");
    }

    public static bool TryDockerComposeOldSyntax(ILogger logger, string dockerComposePath, string projectRoot, string containerName)
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

        return RunComposeProcess(logger, startInfo, containerName, "Docker-compose");
    }

    private static bool RunComposeProcess(ILogger logger, ProcessStartInfo startInfo, string containerName, string commandLabel)
    {
        using var process = Process.Start(startInfo);
        if (process == null)
        {
            logger.LogWarning("Failed to start docker process. Docker may not be installed or accessible.");
            return false;
        }

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit(ProcessWaitMs);

        if (process.ExitCode == 0)
        {
            return true;
        }

        logger.LogWarning("{CommandLabel} command failed with exit code {ExitCode}. Output: {Output}, Error: {Error}",
            commandLabel, process.ExitCode, output, error);
        return false;
    }
}
