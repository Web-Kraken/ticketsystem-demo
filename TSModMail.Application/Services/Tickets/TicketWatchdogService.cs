using DSharpPlus;
using DSharpPlus.Entities;
using MongoDB.Driver.Linq;
using SLog;
using System;
using System.Linq;
using System.Threading.Tasks;
using TSModMail.Application.Helpers;
using TSModMail.Core.Entities;
using TSModMail.Core.Entities.Tickets;
using TSModMail.Core.Repositories;
using TSModMail.Core.Services;
using TSModMail.Core.Services.Tickets;

namespace TSModMail.Application.Services.Tickets;

/// <inheritdoc cref="ITicketWatchdogService"/>
public class TicketWatchdogService : ITicketWatchdogService
{
    private const char Separator = '︱';

    private readonly ITicketRegionRepository _config;
    private readonly ITicketService _tickets;
    private readonly IPermissionService _permissions;
    private readonly DiscordClient _discord;
    private readonly IAsyncLog _log;

    public TicketWatchdogService(
        ITicketRegionRepository config,
        ITicketService tickets,
        IPermissionService permissions,
        DiscordClient client,
        IAsyncLog @base
    )
    {
        _config = config;
        _tickets = tickets;
        _discord = client;
        _permissions = permissions;

        _log = new PrefixedLog(@base, $"[{nameof(TicketWatchdogService).ToUpper()}] ");

        _tickets.OnOpenTicket += HandleTicketOpen;
    }

    private static string GetName(Ticket t, TicketWaitStatus s) => $"{EmojiHelper.GetTSEmoji(s)}{Separator}{t.Name}";

    public Task HandleTicketMessage(DiscordMessage m, Ticket t)
    {
        //TODO: Update on message.
        return Task.CompletedTask;
    }

    public async Task HandleTicketOpen(Ticket ticket)
    {
        var channel = await _discord.GetChannelAsync(ticket.ChannelId);

        if (channel is null)
        {
            // TODO: Probably should warn.
            return;
        }

        var newName = GetName(ticket, TicketWaitStatus.WaitingOnTS);

        await channel.ModifyAsync(act => act.Name = newName);
        await _log.LogDebug($"Updated {channel.Id} name to {newName}.");
    }

    private async Task<TicketWaitStatus> GetStatus(DiscordChannel channel, Ticket ticket)
    {
        var messages = channel.GetMessagesAsync();

        var filtered = messages
            .Where(msg => !msg.Author.IsBot)
            .OrderBy(msg => msg.CreationTimestamp);

        var lastMsg = await filtered.LastOrDefaultAsync();

        if (lastMsg is null)
        {
            return TicketWaitStatus.WaitingOnTS;
        }

        var delta = DateTimeOffset.UtcNow - lastMsg.CreationTimestamp;

        // Not ticket owner == "staff".
        if (lastMsg.Author.Id == ticket.Author.UserId)
        {
            if (delta > TimeSpan.FromHours(8))
            {
                return TicketWaitStatus.WaitingOnTSLate;
            }

            return TicketWaitStatus.WaitingOnTS;
        }

        if (delta > TimeSpan.FromHours(24))
        {
            return TicketWaitStatus.WaitingOnUserLate;
        }

        return TicketWaitStatus.WaitingOnUser;
    }

    public async Task RefreshTicketChannels(TicketRegion config)
    {
        await _log.LogDebug($"Updating channels in {config.Id}.");

        var guild = await _discord.GetGuildAsync(config.GuildId);

        var tickets = await _tickets.GetTicketsFromGuild(guild.Id);

        foreach (var ticket in tickets)
        {
            if (ticket.Status != TicketStageStatus.Open)
            {
                continue;
            }

            var channel = guild.GetChannel(ticket.ChannelId);

            if (channel is null)
            {
                continue;
            }

            var status = await GetStatus(channel, ticket);

            var newName = GetName(ticket, status);
            if (newName == channel.Name)
            {
                continue;
            }

            await _log.LogDebug($"Updating {channel.Name} to {newName}");
            try
            {
                await channel.ModifyAsync(act => act.Name = newName);
            }
            catch (Exception ex)
            {
                await _log.LogError($"Error updating channel name for {channel.Mention} in {guild.Id}.", ex);
            }
            await _log.LogDebug($"Updated {channel.Name} to {newName}");

            // Tragically, avoiding rate limits is cringe.
            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }
}
