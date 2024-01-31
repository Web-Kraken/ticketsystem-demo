using System;
using System.Threading.Tasks;

namespace SLog;

public class ErrorLogHandler : LogBase
{
    private readonly IAsyncLog _defaultLog;
    private readonly IAsyncLog _errorLog;

    public ErrorLogHandler(IAsyncLog defaultLog, IAsyncLog errorLog)
    {
        _defaultLog = defaultLog ?? throw new ArgumentNullException(nameof(defaultLog));
        _errorLog = errorLog ?? throw new ArgumentNullException(nameof(errorLog));
    }

    public override async Task Log(LogSeverity severity, string message, Exception? ex = null)
    {
        if (severity == LogSeverity.Error)
        {
            await _errorLog.Log(severity, message, ex);
        }

        await _defaultLog.Log(severity, message, ex);
    }
}
