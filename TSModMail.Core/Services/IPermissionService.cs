using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TSModMail.Core.Entities;

namespace TSModMail.Core.Services;

/// <summary>
/// A service to get the current Discord users permissions from their id.
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Update the users permission level, or set it if it does not exit.
    /// </summary>
    Task SetPermissionLevel(GuildMemberKey key, BotPermissionLevel level, Guid region);

    /// <summary>
    /// Blacklist a user and prevent them from opening tickets or performing
    /// actions.
    /// </summary>
    Task BlacklistUser(GuildMemberKey victim, DiscordMember author, Guid region);

    /// <summary>
    /// Reinstate a user's permissions, unblacklisting them.
    /// </summary>
    Task ReinstateUser(GuildMemberKey victim, DiscordMember author, Guid region);

    Task<IEnumerable<PermissionRecord>> GetForMember(GuildMemberKey key);

    Task<PermissionRecord?> GetForMember(GuildMemberKey key, Guid regionId);

    Task<bool> IsValidForRegion(GuildMemberKey member, BotPermissionLevel requiredPerm, Guid region);

    Task<IEnumerable<TicketRegion>> GetValidRegions(GuildMemberKey member, BotPermissionLevel level);
}
