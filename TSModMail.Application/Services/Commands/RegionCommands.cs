using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.SlashCommands;
using SLog;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSModMail.Application.Helpers;
using TSModMail.Core.Entities;
using TSModMail.Core.Repositories;
using TSModMail.Core.Services;

namespace TSModMail.Application.Services.Commands;

[SlashCommandGroup("region", "Region related commands.")]
public class RegionCommands : ApplicationCommandModule
{
    private readonly IModalRepository _modals;
    private readonly IPermissionService _permissions;
    private readonly IRegionService _regions;
    private readonly InteractivityExtension _interactivity;
    private readonly IAsyncLog _log;

    public RegionCommands(
        IModalRepository modals,
        IPermissionService permissions,
        IRegionService regions,
        InteractivityExtension interactivity,
        IAsyncLog @base)
    {
        _modals = modals;
        _permissions = permissions;
        _regions = regions;
        _interactivity = interactivity;
        _log = new PrefixedLog(@base, $"[{nameof(RegionCommands).ToUpper()}] ");
    }

    //TODO: This needs to go somewhere else, possibly extension.
    private static string ChannelFormat(ulong id) => $"<#{id}> ({id})";

    [SlashCommand("list", "List all configured regions in this guild.")]
    public async Task Listcommand(InteractionContext ctx)
    {
        await ctx.DeferAsync();

        var regions = await _regions.GetByGuild(ctx.Guild);

        var embed = new DiscordEmbedBuilder()
            .WithTitle(ctx.Guild.Name);

        if (!regions.Any())
        {
            embed.AddField($"**There are**", "Zero, available regions.");
        }
        else
        {
            foreach (var region in regions)
            {
                embed.AddField($"**{region.Name}**", region.Id.ToString());
            }
        }

        await ctx.EditResponseAsync(
            new DiscordWebhookBuilder().AddEmbed(embed)
        );
    }

    [SlashCommand("config", "Print a description of the configuration of this region.")]
    public async Task ConfigCommand(
        InteractionContext ctx,
        [Option("region", "Which region to configure.")]
        string? regionName = null
    )
    {
        await ctx.DeferAsync();

        var region = await _regions.TryGetRegion(regionName, ctx, BotPermissionLevel.Supervisor, _permissions);

        if (region == null)
        {
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle(ctx.Guild.Name)
            .WithAuthor(ctx.User.Username); // TODO: Branding. Make embeds include original author info.


        bool hasAttachments = region.AttachmentConfig != null;
        var attachmentText = hasAttachments
            ? ChannelFormat(region.AttachmentConfig!.ChannelId)
            : "Disabled";
        var logText = region.LogChannelId.HasValue
            ? ChannelFormat(region.LogChannelId.Value)
            : "Disabed";

        embed
            .AddField("Region Id", region.Id.ToString().Limit(6))
            .AddField("Region Name", region.Name)
            .AddField("Guild ID", region.GuildId.ToString())
            .AddField("Ticket Category", ChannelFormat(region.TicketCategoryId))
            .AddField(
                "Backlog Category",
                region.BacklogCategoryId.HasValue ? ChannelFormat(region.BacklogCategoryId.Value) : "DISABLED"
            )
            .AddField("Attachment Channel", attachmentText)
            .AddField("Log Channel", logText);

        if (hasAttachments)
        {
            embed.AddField("Ticket Retention Time", $"{region.AttachmentConfig!.RetentionLength} seconds.");
        }

        var sb = new StringBuilder();

        if (!region.TicketPermissions.Any())
        {
            sb.Append("No permissions configured.");
        }
        else
        {
            sb.AppendLine();
            foreach (var perm in region.TicketPermissions)
            {
                string comma = perm.Deny != 0 ? "," : ""; // Whether comma is used.
                string allow = perm.Allow != 0 ? ($"+{(long)perm.Allow}{comma}") : "";
                string deny = perm.Deny != 0 ? ($"-{(long)perm.Deny}") : "";
                sb.AppendLine($"> <@&{perm.RoleId}> | {perm.RoleId} ({allow}{deny})");
                if (perm.Allow != 0)
                {
                    sb.AppendLine($"**+** {perm.Allow}");
                }
                if (perm.Deny != 0)
                {
                    sb.AppendLine($"**-** {perm.Deny}");
                }
            }
        }

        embed.AddField("Ticket Permissions:", sb.ToString());

        await ctx.EditResponseAsync(
            new DiscordWebhookBuilder().AddEmbed(embed)
        );
    }

    [SlashCommand("addperm", "Add a role permission to be applied at ticket channel opening.")]
    public async Task AddPermissionCommand(
       InteractionContext ctx,
       [Option("role", "Role")] DiscordRole role,
       [Option("grant", "Calculated permissions for this role.")] long grant = 0,
       [Option("deny", "Calculated permissions for this role.")] long deny = 0,
       [Option("region", "Which guild region to apply this to.")] string? regionName = null
   )
    {
        await ctx.DeferAsync();

        var region = await _regions.TryGetRegion(regionName, ctx, BotPermissionLevel.Supervisor, _permissions);

        if (region == null)
        {
            return;
        }

        var existing = region.TicketPermissions.FirstOrDefault(p => p.Id == role.Id);
        if (existing != null)
        {
            await ctx.EditResponsePlain(
                $"A permission record for this role already exists. {existing.Id} - Allow {(long)existing.Allow} Deny {(long)existing.Deny}"
            );
            return;
        }

        var newRecord = new TicketPermissions(role.Id)
        {
            Allow = (Permissions)grant,
            Deny = (Permissions)deny
        };

        region.TicketPermissions.Add(newRecord);

        try
        {
            await _regions.UpdateRegion(region);
        }
        catch (Exception e)
        {
            await _log.LogError($"Could not update Guild config in {ctx.Guild.Id}", e);
            await ctx.EditResponsePlain("Could not update guild config. Changes not applied.");
        }

        await ctx.EditResponsePlain("Role permissions updated.");
    }

    [SlashCommand("removeperm", "Remove a role permission from ticket permissions.")]
    public async Task RemovePermissionCommand(
       InteractionContext ctx,
       [Option("role", "Role")] DiscordRole role,
       [Option("region", "Which region")] string? regionName = null
    )
    {
        await ctx.DeferAsync();

        var region = await _regions.TryGetRegion(regionName, ctx, BotPermissionLevel.Supervisor, _permissions);
        if (region == null)
        {
            return;
        }

        var record = region.TicketPermissions.FirstOrDefault(p => p.Id == role.Id);

        if (record == null)
        {
            await ctx.EditResponsePlain($"There is no permission record for this role. ({role.Id})");
            return;
        }

        region.TicketPermissions.Remove(record);

        try
        {
            await _regions.UpdateRegion(region);
        }
        catch (Exception e)
        {
            await _log.LogError($"Could not update Guild config in {ctx.Guild.Id}", e);
            await ctx.EditResponsePlain("Could not update guild config. Changes not applied.");
        }

        await ctx.EditResponsePlain("Role permissions updated.");
    }

    [SlashCommand("add", "Add a new ticket region in this guild.")]
    public async Task AddCommand(
        InteractionContext ctx,
        [Option("name", "A nickname for this region.")]
        string name,
        [Option("ticketCategory", "Where should tickets be created.")]
        DiscordChannel ticketChannel
    )
    {
        await ctx.DeferAsync(true); //TODO: Check why this can take so long.

        var permsRegions = await _permissions.GetForMember(ctx.Member);
        var isDev = permsRegions.Any(x => x.Level >= BotPermissionLevel.Dev);

        if (!isDev)
        {
            await ctx.EditResponsePlain("You do not have permission to do this.");
            return;
        }

        var newRegion = new TicketRegion(name, ctx.Guild.Id, ticketChannel.Id);

        await _regions.UpdateRegion(newRegion);

        await ctx.EditResponsePlain($"Done.");
    }

    [SlashCommand("panic", "Enable/disable panic mode")]
    public async Task PanicCommand(
        InteractionContext ctx,
        [Option("message", "Panic message")] string? message = null,
        [Option("region", "Which region")] string? regionName = null
    )
    {
        await ctx.DeferAsync(true);

        var region = await _regions.TryGetRegion(regionName, ctx, BotPermissionLevel.Supervisor, _permissions);

        if (region == null)
        {
            return;
        }

        if (region.PanicMessage == null && message == null)
        {
            await ctx.EditResponsePlain("Panic mode is already disabled.");
            return;
        }

        region.PanicMessage = message;

        await _regions.UpdateRegion(region);

        if (message == null)
        {
            await ctx.EditResponsePlain($"Panic mode disabled.");
        }
        else
        {
            await ctx.EditResponsePlain($"Panic message set to `{message}`.");
        }
    }

    [SlashCommand("postbutton", "Post a button in the specified channel.")]
    public async Task PostButton(
        InteractionContext ctx,
        [Option("message", "What should the message say.")] string rawMsg,
        [Option("menu", "Which menu to use.")] string menu,
        [Option("channel", "Which channel to post in.")] DiscordChannel? c = null
    )
    {
        await ctx.DeferAsync();

        var channel = c ?? ctx.Channel;

        var permsRegions = await _permissions.GetForMember(ctx.Member);
        var isDev = permsRegions.Any(x => x.Level >= BotPermissionLevel.Dev);

        if (!isDev)
        {
            await ctx.EditResponsePlain("You do not have permission to do this.");
            return;
        }

        var message = rawMsg.Replace(@"\n", "\n");

        var submitBtn = new DiscordButtonComponent(
            ButtonStyle.Primary,
            menu,
            "Open A Ticket"
        );

        var msgBuilder = new DiscordMessageBuilder()
            .WithContent(message)
            .AddComponents(submitBtn);

        await channel.SendMessageAsync(msgBuilder);

        await ctx.EditResponsePlain("Done.");
    }

    [SlashCommand("postembed", "Post a button and embed in the specified channel.")]
    public async Task PostEmbed(
        InteractionContext ctx,
        [Option("menu", "Which menu to use.")] string menu,
        [Option("title", "Title of the embed to post.")] string title,
        [Option("message", "What should the message say.")] string rawMsg,
        [Option("color", "The hexcode to use for the embed colour.")] string color,
        [Option("button", "The content of the button.")] string buttonText = "Open A Ticket",
        //[Option("emoji", "The emoji to be used on the button.")] DiscordEmoji? emoji = null,
        [Option("channel", "Which channel to post in.")] DiscordChannel? c = null
    )
    {
        await ctx.DeferAsync();

        var channel = c ?? ctx.Channel;

        var permsRegions = await _permissions.GetForMember(ctx.Member);
        var isDev = permsRegions.Any(x => x.Level >= BotPermissionLevel.Dev);

        if (!isDev)
        {
            await ctx.EditResponsePlain("You do not have permission to do this.");
            return;
        }

        var message = rawMsg.Replace(@"\n", "\n");

        var submitBtn = new DiscordButtonComponent(
            ButtonStyle.Primary,
            menu,
            buttonText
        );

        var embed = new DiscordEmbedBuilder()
            .WithTitle(title)
            .WithDescription(message)
            .WithColor(new DiscordColor(color));

        var msg = new DiscordMessageBuilder()
            .WithEmbed(embed)
            .AddComponents(submitBtn);

        await channel.SendMessageAsync(msg);

        await ctx.EditResponsePlain("Done.");
    }
}