using System;
using System.Threading.Tasks;

namespace SLog;

/// <summary>
/// An implementation of <see cref="IAsyncLog"/> that applies a prefix to 
/// a given message.
/// </summary>
public class PrefixedLog : LogBase
{
    private readonly IAsyncLog _log;
    private readonly string _prefix;

    public PrefixedLog(IAsyncLog log, string prefix)
    {
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
    }

    public override Task Log(LogSeverity severity, string message, Exception? ex = null)
    {
        return _log.Log(severity, $"{_prefix}{message}", ex);
    }
}
