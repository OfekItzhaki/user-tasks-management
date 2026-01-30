using Microsoft.Extensions.Logging;

namespace TaskManagement.WindowsService.Helpers;

/// <summary>
/// Creates a temporary logger used before the host is built (e.g. for dependency checks)
/// </summary>
public static class StartupLoggerFactory
{
    public static ILogger CreateProgramLogger()
    {
        return LoggerFactory.Create(loggingBuilder => loggingBuilder
            .AddConsole(options => options.FormatterName = "simple")
            .AddSimpleConsole(options =>
            {
                options.IncludeScopes = false;
                options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                options.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled;
            })).CreateLogger("Program");
    }
}
