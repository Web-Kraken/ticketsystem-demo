using DSharpPlus.Entities;
using System;

namespace TSModMail.Core.Entities;

/// <summary>
/// Relates a UserId to a GuildId. Glorified record.
/// </summary>
public class GuildMemberKey // It would be a struct but MongoDB is retarded.
{
    /// <summary>
    /// Discord Guild Id.
    /// </summary>
    public ulong GuildId { get; private set; }

    /// <summary>
    /// Discord Member Id.
    /// </summary>
    public ulong UserId { get; private set; }

    public GuildMemberKey(ulong gid, ulong uid)
    {
        GuildId = gid;
        UserId = uid;
    }

    // TODO: All of the equality properly.

    public static implicit operator GuildMemberKey(DiscordMember member)
        => new GuildMemberKey(member.Guild.Id, member.Id);

    public static bool operator ==(GuildMemberKey a, GuildMemberKey b)
        => a.Equals(b);

    public static bool operator !=(GuildMemberKey a, GuildMemberKey b)
        => !(a==b);

    public override string ToString() => $"{UserId}@{GuildId}";

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj is GuildMemberKey gmk)
        {
            return gmk.UserId == UserId && gmk.GuildId == GuildId;
        }

        if (obj is DiscordMember dm)
        {
            return dm.Id == UserId && dm.Guild.Id == GuildId;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GuildId, UserId);
    }
}
