using System;
using System.Threading.Tasks;

namespace SLog;

public enum LogSeverity
{
    Debug = -1,
    Info = 0,
    Warning = 1,
    Error = 1 << 1
}

public interface IAsyncLog
{
    Task Log(LogSeverity severity, string message, Exception? ex = null);
    Task LogDebug(string debug);
    Task LogInfo(string info);
    Task LogWarning(string warning);
    Task LogError(string error, Exception? ex);
}