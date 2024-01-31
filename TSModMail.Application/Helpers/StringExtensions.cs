using DSharpPlus.Entities;
using System.Linq;

namespace TSModMail.Application.Helpers;

/// <summary>
/// Implement some functionality for strings that should come out of the box.
/// </summary>
public static class StringHelper
{
    /// <summary>
    /// Only take the first size or less characters from a string.
    /// </summary>
    public static string Limit(this string @this, int size, bool addEllipses = true)
        => @this.Length < size ? @this : addEllipses ? $"{@this.Substring(0, size - 3)}..." : @this.Substring(0, size);

    /// <summary>
    /// Limits the characters used in a string to ASCII only.
    /// </summary>
    public static string LimitAscii(this string @this)
        => string.Join("", @this.ToCharArray().Where(c => c < 127));

    //TODO: Make these into extensions.
    /// <summary>
    /// Displays a verbose version of the member info.
    /// </summary>
    public static string Format(DiscordMember member) => $"@{member.Username} ({member.Id})";

    /// <summary>
    /// Displays a verbose version of the channel info.
    /// </summary>
    public static string Format(DiscordChannel channel) => $"#{channel.Name} ({channel.Id})";

    /// <summary>
    /// Displays a verbose version of the guild info.
    /// </summary>
    public static string Format(DiscordGuild guild) => $"\"{guild.Name}\" ({guild.Id})";

}
