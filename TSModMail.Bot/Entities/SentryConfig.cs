namespace TSModMail.Bot.Entities;

public class SentryConfig
{
    /// <summary>
    /// Sentry reporting url
    /// </summary>
    public string Dsn { get; set; }
    
    /// <summary>
    /// What rate of samples are sent
    /// </summary>
    public double TracesSampleRate { get; set; } = 1.0;
}