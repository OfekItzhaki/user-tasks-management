namespace TaskManagement.WindowsService.Helpers;

/// <summary>
/// Finds project root directory by locating docker-compose.yml
/// </summary>
public static class ProjectRootFinder
{
    public static string? FindProjectRoot()
    {
        // Strategy 1: Start from AppContext.BaseDirectory (executable location)
        var projectRoot = FindFromDirectory(AppContext.BaseDirectory);
        if (projectRoot != null) return projectRoot;

        // Strategy 2: Try from current working directory
        projectRoot = FindFromDirectory(Directory.GetCurrentDirectory());
        if (projectRoot != null) return projectRoot;

        // Strategy 3: Try common project root patterns
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
                return possibleRoot;
            }
        }

        return null;
    }

    private static string? FindFromDirectory(string startDirectory)
    {
        var currentDir = startDirectory;

        // Navigate up from bin/Debug/net8.0 or bin/Release/net8.0
        for (int i = 0; i < 8; i++)
        {
            var dockerComposePath = Path.Combine(currentDir, "docker", "docker-compose.yml");
            if (File.Exists(dockerComposePath))
            {
                return currentDir;
            }

            var parent = Path.GetDirectoryName(currentDir);
            if (string.IsNullOrEmpty(parent) || parent == currentDir)
                break;
            currentDir = parent;
        }

        return null;
    }
}
