using System;

namespace TSModMail.Application.Helpers;

/// <summary>
/// Implement some functionality that should come out of the box.
/// </summary>
internal static class TimeExtensions
{
    /// <summary>
    /// Convert a <see cref="DateTime"/> to a unix seconds timestamp.
    /// </summary>
    public static ulong ToUnixTimeSeconds(this DateTime @this)
    {
        return (ulong)((DateTimeOffset)@this).ToUnixTimeSeconds();
    }

    public static string ToDiscordTimestmap(this DateTime @this)
    {
        return $"<t:{@this.ToUnixTimeSeconds()}:F>";
    }
}
