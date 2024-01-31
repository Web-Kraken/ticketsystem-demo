using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TSModMail.Core.Entities;
using TSModMail.Core.Entities.Tickets;

namespace TSModMail.Core.Repositories;

/// <summary>
/// Repository for <see cref="HistoricTicket"/>s.
/// </summary>
public interface ITicketHistoryRepository
    : ICrudBase<HistoricTicket, Guid>
{

    Task<IEnumerable<HistoricTicket>> GetHistory(GuildMemberKey key);

    /// <summary>
    /// Get historic tickets for a user in a guild.
    /// </summary>
    Task<IEnumerable<HistoricTicket>> GetHistoryRangeForUser(ulong guildId, ulong participantId, DateTime start, DateTime end);

    /// <summary>
    /// Get historic tickets for an entire guild.
    /// </summary>
    Task<IEnumerable<HistoricTicket>> GetHistoryRangeForGuild(ulong guildId, DateTime start, DateTime end);
}
