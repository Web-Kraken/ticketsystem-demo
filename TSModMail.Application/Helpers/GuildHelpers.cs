using DSharpPlus;
using DSharpPlus.Entities;

namespace TSModMail.Application.Helpers;

/// <summary>
/// Helpers for some guilds.
/// </summary>
public static class GuildHelpers
{
    private const int MEGABYTE = 1024 * 1024;

    /// <summary>
    /// Get the max upload size of a guild.
    /// </summary>
    public static int GetMaxFileSize(DiscordGuild guild)
    {
        if (guild.PremiumTier == PremiumTier.Tier_3)
        {
            return MEGABYTE * 100;
        }

        if (guild.PremiumTier == PremiumTier.Tier_2)
        {
            return MEGABYTE * 50;
        }

        return MEGABYTE * 8;
    }
}
