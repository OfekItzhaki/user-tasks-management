using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using TaskManagement.WindowsService.Configuration;

namespace TaskManagement.WindowsService.Helpers;

/// <summary>
/// Creates a temporary logger used before the host is built (e.g. for dependency checks)
/// </summary>
public static class StartupLoggerFactory
{
    public static ILogger CreateProgramLogger()
    {
        return LoggerFactory.Create(loggingBuilder =>
        {
            loggingBuilder.Services.AddSingleton<ConsoleFormatter, CleanConsoleFormatter>();
            loggingBuilder.AddConsole(options => options.FormatterName = "clean");
        }).CreateLogger("Program");
    }
}
