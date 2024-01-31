using DSharpPlus.Entities;
using SLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TSModMail.Core.Entities;
using TSModMail.Core.Repositories;
using TSModMail.Core.Services;

namespace TSModMail.Application.Services;

public class RegionService : IRegionService
{
    private readonly ITicketRegionRepository _regions;
    private readonly IAsyncLog _log;

    public RegionService(ITicketRegionRepository regions, IAsyncLog @base)
    {
        _regions = regions;
        _log = new PrefixedLog(@base, $"[{nameof(RegionService).ToUpper()}] ");
    }

    public async Task CreateRegion(string name, DiscordGuild guild, DiscordChannel ticketChannel)
    {
        var region = new TicketRegion(name, guild.Id, ticketChannel.Id);

        await _regions.Insert(region);

        await _log.LogInfo($"Created region \"{name}\" ({region.Id}) in {guild.Name} ({guild.Id})");
    }

    public async Task<IEnumerable<TicketRegion>> GetByGuild(DiscordGuild guild)
    {
        await _log.LogDebug($"Getting all regions in {guild.Name} ({guild.Id})");

        var regions = await _regions.GetFromGuild(guild.Id);

        return regions;
    }

    public async Task<TicketRegion?> GetByName(DiscordGuild guild, string name)
    {
        await _log.LogDebug($"Getting region {name} in {guild.Name} ({guild.Id})");

        var region = await _regions.GetByName(guild.Id, name);

        return region;
    }

    public async Task UpdateRegion(TicketRegion region)
    {
        await _log.LogDebug($"Updated region {region.Name} in {region.GuildId}");

        await _regions.Update(region);
    }

    public async Task DeleteRegion(TicketRegion region)
    {
        await _regions.Delete(region);

        await _log.LogInfo($"Deleted region \"{region.Name}\" ({region.Id}) from {region.GuildId})");
    }

    public async Task<TicketRegion?> GetByGuid(Guid regionId)
    {
        await _log.LogDebug($"Fetching region by id {regionId}.");

        return await _regions.GetById(regionId);
    }
}
