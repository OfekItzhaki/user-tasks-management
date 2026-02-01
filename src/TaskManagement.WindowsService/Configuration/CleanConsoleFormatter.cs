using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using System.IO;

namespace TaskManagement.WindowsService.Configuration;

/// <summary>
/// Custom console formatter that displays only timestamp and message without category names
/// </summary>
public sealed class CleanConsoleFormatter : ConsoleFormatter
{
    public CleanConsoleFormatter() : base("clean")
    {
    }

    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter)
    {
        string? message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
        if (message is null)
        {
            return;
        }

        // Write timestamp
        var timestamp = DateTime.Now.ToString("[HH:mm:ss]");
        textWriter.Write(timestamp);
        textWriter.Write(' ');

        // Write message only (no category, no level, no event ID)
        textWriter.WriteLine(message);

        // Write exception if present
        if (logEntry.Exception != null)
        {
            textWriter.WriteLine(logEntry.Exception.ToString());
        }
    }
}
