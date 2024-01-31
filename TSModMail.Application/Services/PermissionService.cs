using DSharpPlus.Entities;
using SLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TSModMail.Core.Entities;
using TSModMail.Core.Repositories;
using TSModMail.Core.Services;

namespace TSModMail.Application.Services;

/// <inheritdoc cref="IPermissionService"/>
public class PermissionService : IPermissionService
{
    private readonly IRegionService _regions;
    private readonly IPermissionRepository _permRepo;
    private readonly IAsyncLog _log;

    public PermissionService(IRegionService region, IPermissionRepository permRepo, IAsyncLog @base)
    {
        _regions = region;
        _permRepo = permRepo;
        _log = new PrefixedLog(@base, $"[{nameof(PermissionService).ToUpper()}] ");
    }

    public async Task BlacklistUser(GuildMemberKey victim, DiscordMember author, Guid region)
    {
        await _log.LogInfo(
            $"Blacklisting user {victim.UserId} in {victim.GuildId} requested by {author.Id}."
        );

        var record = await _permRepo.GetForMember(victim, region) ?? new PermissionRecord(victim, region);

        record.Update(
            BotPermissionLevel.Blacklisted,
            $"Blacklist requested by {author.Username} ({author.Id})");

        await _permRepo.Update(record);
    }

    public async Task ReinstateUser(GuildMemberKey victim, DiscordMember author, Guid region)
    {
        await _log.LogInfo(
            $"Reinstating user {victim.UserId} in {victim.GuildId} requested by {author.Id}."
        );

        var record = await _permRepo.GetForMember(victim, region);

        if (record == null || record.Level != BotPermissionLevel.Blacklisted)
        {
            return;
        }

        record.Update(
            BotPermissionLevel.User,
            $"Reinstatement requested by {author.Username} ({author.Id})");

        await _permRepo.Update(record);
    }

    public async Task<PermissionRecord?> GetForMember(GuildMemberKey key, Guid region)
    {
        await _log.LogDebug($"Getting permission level for {key.UserId}.");

        // TODO: this can never return nullable
        var record = await _permRepo.GetForMember(key, region) ?? new PermissionRecord(key, region);

        return record;
    }

    public async Task<IEnumerable<PermissionRecord>> GetForMember(GuildMemberKey key)
    {
        await _log.LogDebug($"Getting permission level for {key.UserId}.");

        var record = await _permRepo.GetForMember(key);

        return record;
    }

    public async Task SetPermissionLevel(GuildMemberKey key, BotPermissionLevel level, Guid region)
    {
        await _log.LogDebug($"Setting Permission level for {key.UserId}.");

        var record = await _permRepo.GetForMember(key, region) ?? new PermissionRecord(key, region);

        record.Update(level, $"Promoted at {DateTime.UtcNow} UTC");

        await _permRepo.Update(record);
    }

    public async Task<bool> IsValidForRegion(GuildMemberKey member, BotPermissionLevel requiredPerm, Guid region)
    {
        await _log.LogDebug($"Checking {member} for perm {requiredPerm} for {region}.");

        var perm = await _permRepo.GetForMember(member, region);

        return perm != null && perm.Level >= requiredPerm;
    }

    public async Task<IEnumerable<TicketRegion>> GetValidRegions(GuildMemberKey member, BotPermissionLevel level)
    {
        await _log.LogDebug($"Getting valid regions for {member} for {level}.");

        var perms = await _permRepo.GetForMember(member);

        var validPerms = perms.Where(x => x.Level >= level);

        var regionTasks = await Task.WhenAll(validPerms.Select(x => _regions.GetByGuid(x.Region)));

        return regionTasks
            .Where(x => x is not null)
            .Select(x => x!);
    }
}