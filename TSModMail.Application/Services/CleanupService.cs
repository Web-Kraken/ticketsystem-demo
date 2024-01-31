using DSharpPlus;
using SLog;
using System;
using System.Linq;
using System.Threading.Tasks;
using TSModMail.Core.Entities;
using TSModMail.Core.Entities.Tickets;
using TSModMail.Core.Repositories;
using TSModMail.Core.Services;
using TSModMail.Core.Services.Tickets;

namespace TSModMail.Application.Services;

/// <inheritdoc cref="ICleanupService"/>
public class CleanupService : ICleanupService
{
    private readonly ITicketRegionRepository _config;
    private readonly DiscordClient _discord;
    private readonly ITicketService _tickets;
    private readonly IAttachmentService _attachment;
    private readonly IAsyncLog _log;

    public CleanupService(
        ITicketRegionRepository config,
        DiscordClient client,
        ITicketService tickets,
        IAttachmentService attachment,
        IAsyncLog @base
        )
    {
        _config = config;
        _discord = client;

        _log = new PrefixedLog(@base, $"[{nameof(CleanupService).ToUpper()}] ");
        _tickets = tickets;
        _attachment = attachment;
    }

    public async Task CleanupQuestioningTickets(TicketRegion config)
    {
        int deleteCount = 0;

        var guild = await _discord.GetGuildAsync(config.GuildId);

        var tickets = await _tickets.GetTicketsFromGuild(guild.Id);

        foreach (var ticket in tickets.Where(ticket => ticket.Status < TicketStageStatus.Open))
        {
            var channel = guild.GetChannel(ticket.ChannelId);

            if (channel is null)
            {
                continue;
            }

            DateTimeOffset timeToCheck;

            var msgs = channel.GetMessagesAsync(1);

            var last = await msgs.FirstOrDefaultAsync();
            if (last is not null)
            {
                timeToCheck = last.CreationTimestamp;
            }
            else // In case no messages / bugged channel.
            {
                timeToCheck = channel.CreationTimestamp;
            }

            var delta = DateTimeOffset.UtcNow - timeToCheck;
            if (delta < TimeSpan.FromMinutes(5))
            {
                continue;
            }

            await _tickets.AbortTicket(ticket);
            deleteCount++;
        }

        await _log.LogInfo($"Cleaned up {deleteCount} stale ticket{(deleteCount == 1 ? "" : "s")}.");
    }
    public Task CleanupOldTicketLogs(TicketRegion guildConfig)
    {
        //TODO: Cleanup ticket logs
        return Task.CompletedTask;
    }

    public Task CleanupOldAttachments(TicketRegion guildConfig)
    {
        //TODO: Cleanup attachments
        return Task.CompletedTask;
    }
}
