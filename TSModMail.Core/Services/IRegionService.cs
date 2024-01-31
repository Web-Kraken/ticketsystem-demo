using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TSModMail.Core.Entities;

namespace TSModMail.Core.Services;

/// <summary>
/// A service that controls operation of <see cref="TicketRegion"/>.
/// </summary>
public interface IRegionService
{
    /// <summary>
    /// Create a new region in a guild with the minimum configured information.
    /// </summary>
    Task CreateRegion(string name, DiscordGuild guild, DiscordChannel ticketChannel);
    /// <summary>
    /// Get all regions in a guild.
    /// </summary>
    Task<IEnumerable<TicketRegion>> GetByGuild(DiscordGuild guild);
    /// <summary>
    /// Get a specific region in a guild.
    /// </summary>
    Task<TicketRegion?> GetByName(DiscordGuild guild, string name);
    /// <summary>
    /// Get a specific region by its ID
    /// </summary>
    Task<TicketRegion?> GetByGuid(Guid regionId);
    /// <summary>
    /// Update a region.
    /// </summary>
    Task UpdateRegion(TicketRegion region);
    /// <summary>
    /// Remove a region, does not delete channels.
    /// </summary>
    Task DeleteRegion(TicketRegion region);

}
