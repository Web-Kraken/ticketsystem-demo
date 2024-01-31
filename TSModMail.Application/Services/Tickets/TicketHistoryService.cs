using DSharpPlus;
using SLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TSModMail.Core.Entities;
using TSModMail.Core.Entities.Tickets;
using TSModMail.Core.Repositories;
using TSModMail.Core.Services.Tickets;

namespace TSModMail.Application.Services.Tickets;

/// <inheritdoc cref="ITicketHistoryService"/>
public class TicketHistoryService : ITicketHistoryService
{
    private readonly ITicketHistoryRepository _history;
    private readonly DiscordClient _client;
    private readonly ITicketRegionRepository _config;
    private readonly IAsyncLog _log;

    public TicketHistoryService(
        ITicketHistoryRepository history,
        IAsyncLog @base,
        DiscordClient client,
        ITicketRegionRepository config
        )
    {
        _history = history;
        _client = client;
        _config = config;
        _log = new PrefixedLog(@base, $"[{nameof(TicketHistoryService).ToUpper()}] ");
    }

    public async Task<IEnumerable<HistoricTicket>> GetHistoricTicketsForUser(GuildMemberKey key)
    {
        await _log.LogDebug($"Getting historic tickets for {key}");
        return await _history.GetHistory(key);
    }

    public async Task<IEnumerable<HistoricTicket>> GetHistoryRange(
        ulong guildId,
        DateTime start,
        DateTime end
    )
    {
        await _log.LogDebug($"Getting history range in {guildId} [{start} - {end}]");
        return await _history.GetHistoryRangeForGuild(guildId, start, end);
    }

    public async Task<IEnumerable<HistoricTicket>> GetHistoryRangeForUser(
        ulong guildId,
        ulong participantId,
        DateTime start,
        DateTime end
    )
    {
        await _log.LogDebug($"Getting history range for {participantId} @ {guildId} [{start} - {end}]");
        return await _history.GetHistoryRangeForUser(guildId, participantId, start, end);
    }

    public async Task LogHistoricTicket(HistoricTicket ticket)
    {
        await _log.LogDebug($"Adding historic ticket id {ticket.Id}");
        await _history.Insert(ticket);
    }
}
