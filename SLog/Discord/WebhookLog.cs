
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SLog.Discord;

public class WebhookLog : LogBase
{
    private readonly DiscordWebhook _webhook;

    public WebhookLog(DiscordWebhook webhook)
    {
        _webhook = webhook;
    }

    public override async Task Log(LogSeverity sev, string message, Exception? ex = null)
    {
        var content = $"{DateTime.UtcNow} [{sev}] {message} {(ex == null ? "" : $"\n - {ex.Message}")}";

        var parts = SplitAt(content, 2000);

        foreach (var part in parts)
        {
            await _webhook.Send(part);
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    private static IEnumerable<string> SplitAt(string str, int size)
    {
        while (str.Length > 0)
        {
            yield return new string(str.Take(size).ToArray());
            str = new string(str.Skip(size).ToArray());
        }
    }
}
