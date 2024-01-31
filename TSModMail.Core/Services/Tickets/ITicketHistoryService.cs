using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TSModMail.Core.Entities;
using TSModMail.Core.Entities.Tickets;

namespace TSModMail.Core.Services.Tickets;

/// <summary>
/// Service for saving and retrieving historic ticket information.
/// </summary>
public interface ITicketHistoryService
{
    /// <summary>
    /// Save information about a historic ticket.
    /// </summary>
    Task LogHistoricTicket(HistoricTicket ticket);

    /// <summary>
    /// Get the ticket history for a member.
    /// </summary>
    Task<IEnumerable<HistoricTicket>> GetHistoricTicketsForUser(GuildMemberKey key);

    /// <summary>
    /// Get the ticket history for a ticket participant. 
    /// A participant is anyone who has sent a message in a ticket.
    /// </summary>
    Task<IEnumerable<HistoricTicket>> GetHistoryRangeForUser(
        ulong guildId,
        ulong participantId,
        DateTime start,
        DateTime end
    );

    /// <summary>
    /// Get all tickets closed in a particular range.
    /// </summary>
    Task<IEnumerable<HistoricTicket>> GetHistoryRange(
        ulong guildId,
        DateTime start,
        DateTime end
    );
}
