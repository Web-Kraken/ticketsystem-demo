using TSModMail.Core.Entities;
using TSModMail.Repositories.MongoDb.Entities;

namespace TSModMail.Bot.Entities;

/// <summary>
/// Config for the ticket bot.
/// </summary>
internal class TicketBotConfig
{
    /// <summary>
    /// Discord Configuration
    /// </summary>
    public DiscordConfig Discord { get; set; }

    /// <summary>
    /// MongoDb Configuration
    /// </summary>
    public MongoConfig? Mongo { get; set; } = null;

    /// <summary>
    /// Sentry Configuration
    /// </summary>
    public SentryConfig? Sentry { get; set; } = null;

    /// <summary>
    /// Web Api config
    /// </summary>
    //public WebApiConfig? Web { get; set; } = null;

}