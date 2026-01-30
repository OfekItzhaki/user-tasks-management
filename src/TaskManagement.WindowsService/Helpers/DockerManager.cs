using Microsoft.Extensions.Logging;

namespace TaskManagement.WindowsService.Helpers;

/// <summary>
/// Facade for Docker Desktop lifecycle: running check and stuck-state recovery
/// </summary>
public static class DockerManager
{
    public static bool IsDockerRunning(ILogger logger) => DockerRunningChecker.IsDockerRunning(logger);

    public static bool TestAndFixDockerStuckState(ILogger logger) => DockerStuckStateHandler.TestAndFixDockerStuckState(logger);
}
