using System;
using System.Threading.Tasks;

namespace SLog;

/// <summary>
/// A console implementation of <see cref="IAsyncLog"/>.
/// </summary>
public class ConsoleLog : LogBase
{
    public override Task Log(LogSeverity severity, string message, Exception? ex = null)
    {
        string sev = (Enum.GetName(typeof(LogSeverity), severity) ?? "UNKNOWN").ToUpperInvariant();

        var colour = Console.ForegroundColor;

        switch (severity)
        {
            case LogSeverity.Debug:
                Console.ForegroundColor = ConsoleColor.Blue; break;
            case LogSeverity.Error:
                Console.ForegroundColor = ConsoleColor.Red; break;
            case LogSeverity.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow; break;
        }

        Console.WriteLine($"{DateTime.UtcNow} [{sev}] {message} {(ex == null ? "" : $"\n - {ex.Message}")}");

        Console.ForegroundColor = colour;

        return Task.CompletedTask;
    }
}