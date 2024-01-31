namespace TSModMail.Core.Entities;

public class DiscordConfig
{
    /// <summary>
    /// Discord bot auth token.
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// Discord webhook for logging.
    /// </summary>
    public string WebhookUrl { get; set; }
}
