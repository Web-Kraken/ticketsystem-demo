using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using SLog;
using System;
using System.Threading.Tasks;
using TSModMail.Application.Helpers;
using TSModMail.Core.Entities;
using TSModMail.Core.Services;

namespace TSModMail.Application.Services.Commands;

[SlashCommandGroup("permission", "Permissions related commands.")]
public class PermissionCommands : ApplicationCommandModule
{
    private readonly IPermissionService _permissions;
    private readonly IRegionService _regions;
    private readonly IAsyncLog _log;

    public PermissionCommands(
        IPermissionService permissions,
        IRegionService regions,
        IAsyncLog @base
    )
    {
        _permissions = permissions;
        _regions = regions;

        _log = new PrefixedLog(@base, $"[{nameof(PermissionCommands).ToUpper()}] ");
    }

    [SlashCommand(
        "rank",
        "Set a users rank."
    )]
    public async Task SetRankCommand(
        InteractionContext ctx,
        [Option("user", "The user.")] DiscordUser user,
        [Option("rank", "The rank.")] BotPermissionLevel role,
        [Option("region", "The region.")] string? regionName = null
    )
    {
        await ctx.DeferAsync();

        if (user is null)
        {
            await ctx.EditResponsePlain(
                "The user is null."
            );
            return;
        }

        var region = await _regions.TryGetRegion(regionName, ctx, BotPermissionLevel.Supervisor, _permissions);
        if (region is null)
        {
            return;
        }

        PermissionRecord executorPerms = (await _permissions.GetForMember(ctx.Member, region.Id))!;

        var victim = await ctx.Guild.GetMemberAsync(user.Id);
        if (victim is null)
        {
            await ctx.EditResponsePlain(
                "You must specify an actual member."
            );
            return;
        }


        var victimPerms = await _permissions.GetForMember(victim, region.Id);
        if (victimPerms is not null && victimPerms.Level >= executorPerms.Level)
        {
            await ctx.EditResponsePlain(
                "They outrank you."
            );
            return;
        }

        try
        {
            await _permissions.SetPermissionLevel(victim, role, region.Id);
        }
        catch (Exception ex)
        {
            await _log.LogError($"Error setting permission level for {victim.Id}", ex);
            await ctx.EditResponsePlain("Error promoting user.");
        }

        await ctx.EditResponsePlain($"Updated permissions for {victim.Mention}.");
    }


    [SlashCommand(
        "unblacklist",
        "Unlikely to be used."
    )]
    public async Task UnblacklistCommand(
        InteractionContext ctx,
        [Option("user", "The user to be blacklisted.")] DiscordUser user,
        [Option("region", "The region.")] string? regionName = null
    )
    {
        await ctx.DeferAsync();

        if (user is null)
        {
            await ctx.EditResponsePlain(
                "The user is null."
            );
            return;
        }

        var region = await _regions.TryGetRegion(regionName, ctx, BotPermissionLevel.Supervisor, _permissions);
        if (region is null)
        {
            return;
        }

        var victim = await ctx.Guild.GetMemberAsync(user.Id);
        if (victim is null)
        {
            await ctx.EditResponsePlain(
                "You must specify an actual member."
            );
            return;
        }

        var victimPerms = await _permissions.GetForMember(victim, region.Id);
        if (victimPerms is null || victimPerms.Level != BotPermissionLevel.Blacklisted)
        {
            await ctx.EditResponsePlain(
                "They're not blacklisted."
            );
            return;
        }

        try
        {
            await _permissions.ReinstateUser(victim, ctx.Member, region.Id);
        }
        catch (Exception ex)
        {
            await ctx.EditResponsePlain(
                "Error while unblacklisting user..."
            );
            await _log.LogError("Error blacklisting user", ex);
            return;
        }

        await ctx.EditResponsePlain(
            $"Reinstated user {user}"
        );
    }

    [SlashCommand(
        "blacklist",
        "Blacklist a user"
    )]
    public async Task BlacklistCommand(
        InteractionContext ctx,
        [Option("user", "The user to be blacklisted.")] DiscordUser user,
        [Option("region", "The region.")] string? regionName = null
    )
    {
        await ctx.DeferAsync();

        if (user is null)
        {
            await ctx.EditResponsePlain(
                "The user is null. "
            );
            return;
        }

        var region = await _regions.TryGetRegion(regionName, ctx, BotPermissionLevel.Supervisor, _permissions);
        if (region is null)
        {
            return;
        }

        var executorPerms = (await _permissions.GetForMember(ctx.Member, region.Id))!;

        var victim = await ctx.Guild.GetMemberAsync(user.Id);
        if (victim is null)
        {
            await ctx.EditResponsePlain(
                "You must specify an actual member."
            );
            return;
        }

        var victimPerms = await _permissions.GetForMember(victim, region.Id);
        if (victimPerms is not null && victimPerms.Level >= executorPerms.Level)
        {
            await ctx.EditResponsePlain(
                "They outrank you."
            );
            return;
        }

        try
        {
            await _permissions.BlacklistUser(victim, ctx.Member, region.Id);
        }
        catch (Exception ex)
        {
            await ctx.EditResponsePlain(
                "Error while blacklisting user..."
            );
            await _log.LogError("Error blacklisting user", ex);
        }

        await ctx.EditResponsePlain(
            $"Blacklisted user {user}"
        );
    }

    [SlashCommand(
        "check",
        "Check a user's permissions."
    )]
    public async Task CheckPermissionsCommand(
        InteractionContext ctx,
        [Option("user", "Whomst?")] DiscordUser user,
        [Option("region", "Wherest?")] string? regionName = null
    )
    {
        await ctx.DeferAsync();

        var member = await ctx.Guild.GetMemberAsync(user.Id);

        var region = await _regions.TryGetRegion(regionName, ctx, BotPermissionLevel.Assistant, _permissions);
        if (region is null)
        {
            return;
        }

        var perms = await _permissions.GetForMember(member, region.Id);

        if (member is null)
        {
            await ctx.EditResponsePlain("User does not exist.");
            return;
        }

        await ctx.EditResponseAsync(
            new DiscordWebhookBuilder()
                .AddEmbed(
                    new DiscordEmbedBuilder()
                        .WithTitle($"Permissions for {member.Username}")
                        .WithDescription($"__**User**__: {member.Mention}\n\n" +
                        $"__**Role**__: {Enum.GetName(perms!.Level)} ({(byte)perms.Level})\n" +
                        $"__**Reason**__: `{perms.LastUpdateReason.Replace("`", "\\`")}`\n" +
                        $"__**Last Update**__: <t:{perms.LastUpdate.ToUnixTimeSeconds()}:F>")
                )
        );
    }

}
