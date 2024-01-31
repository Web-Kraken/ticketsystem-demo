using System;
using System.Threading.Tasks;

namespace SLog;

/// <summary>
/// Abstract base implementation for <see cref="IAsyncLog"/>.
/// </summary>
public abstract class LogBase : IAsyncLog
{
    public abstract Task Log(LogSeverity severity, string message, Exception? ex = null);

    public Task LogDebug(string debug) => Log(LogSeverity.Debug, debug);

    public Task LogInfo(string info) => Log(LogSeverity.Info, info);

    public Task LogWarning(string warning) => Log(LogSeverity.Warning, warning);

    public Task LogError(string error, Exception? ex) => Log(LogSeverity.Error, error, ex);
}