using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using SLog;
using System;
using System.Linq;
using System.Threading.Tasks;
using TSModMail.Application.Helpers;
using TSModMail.Core.Entities;
using TSModMail.Core.Entities.Tickets;
using TSModMail.Core.Services;
using TSModMail.Core.Services.Tickets;

namespace TSModMail.Application.Services.Commands;

[SlashCommandGroup("ticket", "Ticket related commands.")]
public class TicketCommands : ApplicationCommandModule
{
    private readonly IPermissionService _permissions;
    private readonly ITicketService _tickets;
    private readonly IRegionService _regions;
    private readonly IAsyncLog _log;

    public TicketCommands(
        IPermissionService permissions,
        ITicketService tickets,
        IRegionService regions,
        IAsyncLog @base
    )
    {
        _permissions = permissions;
        _tickets = tickets;
        _regions = regions;
        _log = new PrefixedLog(@base, $"[{nameof(TicketCommands).ToUpper()}] ");
    }

    public async Task MoveChannel(
        InteractionContext ctx,
        DiscordChannel channel,
        bool toBacklog = true
    )
    {
        await ctx.DeferAsync();

        var ticket = await _tickets.GetTicketFromChannel(channel.Id);

        if (ticket == null)
        {
            await ctx.EditResponsePlain($"{channel.Mention} does not appear to be a ticket channel.");
            return;
        }

        var region = await _regions.GetByGuid(ticket.CreationRegion);

        if (region == null)
        {
            return;
        }

        if (region.BacklogCategoryId is null)
        {
            await ctx.EditResponsePlain("This has not enabled ticket backlogging.");
            return;
        }

        var guild = ctx.Guild;

        var sourceId = toBacklog ? region.TicketCategoryId : region.BacklogCategoryId.Value;
        var sourceCategory = guild.GetChannel(sourceId);

        var destId = toBacklog ? region.BacklogCategoryId.Value : region.TicketCategoryId;
        var destCategory = guild.GetChannel(destId);

        var ticketChannel = guild.GetChannel(ticket.ChannelId);

        if (ticketChannel is null)
        {
            await ctx.EditResponsePlain("The channel for this ticket does not exist.");
            return;
        }

        if (sourceCategory is null)
        {
            await ctx.EditResponsePlain($"Source category {sourceId} does not exist.");
            return;
        }

        if (destCategory is null)
        {
            await ctx.EditResponsePlain($"Destination category {destCategory} does not exist.");
            return;
        }

        if (destCategory.Children.Count > 49)
        {
            await ctx.EditResponsePlain($"Destination category {destCategory} is full.");
            return;
        }

        try
        {
            await ticketChannel.ModifyAsync(cem => cem.Parent = destCategory);
        }
        catch (Exception e)
        {
            await _log.LogError($"Could not move ticket {ticket.Id} channel {channel.Id}", e);
            await ctx.EditResponsePlain("Error moving channel has been logged.");
            return;
        }

        try
        {
            await ctx.EditResponsePlain($"{ticket.Mention} has been backlogged.");
        }
        catch (NotFoundException)
        {
            // The command was used in a closed ticket. This is not an issue.
            // TODO: What
        }

        //TODO: Alert the user what it means.
    }

    [SlashCommand("backlog", "Move a ticket to the backlog.")]
    public Task BacklogCommand(
        InteractionContext ctx,
        [Option("ticket", "Ticket to backlog.")]
        DiscordChannel channel
    )
    {
        return MoveChannel(ctx, channel, true);
    }

    [SlashCommand("stats", "Open ticket stats")]
    public async Task StatsCommand(
        InteractionContext ctx,
        [Option("region", "The region.")] string? regionName = null
    )
    {
        await ctx.DeferAsync();

        var region = await _regions.TryGetRegion(regionName, ctx, BotPermissionLevel.Assistant, _permissions);
        if (region == null)
        {
            return;
        }

        var tickets = await _tickets.GetTicketsFromGuild(ctx.Guild.Id);
        var totalTickets = tickets.Count();

        var embed = new DiscordEmbedBuilder();
        embed.WithTitle(ctx.Guild.Name);
        embed.AddField("Total Tickets", $"{totalTickets} Ticket{(totalTickets == 1 ? "" : 's')} open.");

        var mainCount = ctx.Guild.GetChannel(region.TicketCategoryId).Children.Count;
        embed.AddField("Active Tickets: ", $"{mainCount} Ticket{(mainCount == 1 ? "" : 's')} open.");

        if (region.BacklogCategoryId.HasValue)
        {
            var backlogCount = ctx.Guild.GetChannel(region.BacklogCategoryId.Value).Children.Count;
            embed.AddField("Backlog Tickets: ", $"{backlogCount} Ticket{(backlogCount == 1 ? "" : 's')} open.");
        }

        await ctx.EditResponseAsync(
            new DiscordWebhookBuilder()
                .AddEmbed(embed)
            );
    }

    [SlashCommand("close", "Close a ticket thread.")]
    public async Task CloseTicketCommand(
        InteractionContext ctx,
        [Option("issue", "Short summary of the issue the user faced.")] string issue,
        [Option("resolution", "The fix that was applied.")] string resolution,
        [Option("ticket_id", "The ID of the ticket to close.")] DiscordChannel? ticketChannel = null
    )
    {
        var member = await ctx.Guild.GetMemberAsync(ctx.User.Id);

        if (string.IsNullOrWhiteSpace(issue))
        {
            await ctx.RespondEphemeral("Specify the issue.");
            return;
        }

        if (string.IsNullOrWhiteSpace(resolution))
        {
            await ctx.RespondEphemeral("Specify the resolution.");
            return;
        }

        Ticket? ticket;
        if (ticketChannel is null || ctx.Guild.Id != ticketChannel.GuildId)
        {
            ticket = await _tickets.GetTicketFromChannel(ctx.Channel.Id);
        }
        else
        {
            ticket = await _tickets.GetTicketFromChannel(ticketChannel.Id);
        }

        if (ticket is null)
        {
            await ctx.RespondEphemeral("Specific channel is not a ticket channel.");
            return;
        }

        var permissions = await _permissions.GetForMember(ctx.Member, ticket.CreationRegion);
        if (permissions is null || permissions.Level < BotPermissionLevel.Assistant)
        {
            await ctx.RespondEphemeral("You do not have permission to use this.");
            return;
        }

        var reason = $"{issue} -> {resolution}{(resolution.EndsWith(".") ? "" : ".")}";

        await _tickets.CloseTicket(new TicketCloseRequest(ticket, member, reason));

        if (ctx.Channel.Id != ticket.ChannelId) // Don't try and post in closed ticket.
        {
            await ctx.RespondEphemeral("Ticket closed.");
        }
    }
}
