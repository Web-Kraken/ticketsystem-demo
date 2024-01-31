using System.Collections.Generic;
using System.Threading.Tasks;
using TSModMail.Core.Entities;
using TSModMail.Core.Entities.Tickets;

namespace TSModMail.Core.Services.Tickets;

/// <summary>
/// Service for ticket creation, opening and closing.
/// </summary>
public interface ITicketService
{
    public delegate Task OpenTicketHandler(Ticket ticket);

    public OpenTicketHandler OnOpenTicket { get; set; }

    /// <summary>
    /// Open a ticket.
    /// </summary>
    Task<Ticket> CreateTicket(TicketCreateRequest tcr);

    /// <summary>
    /// Get all tickets.
    /// </summary>
    Task<IEnumerable<Ticket>> GetAll();

    /// <summary>
    /// Get ticket from channel id.
    /// </summary>
    Task<Ticket?> GetTicketFromChannel(ulong channelId);

    /// <summary>
    /// Get ticket from member. //TODO: WILL CHANGE
    /// </summary>
    Task<Ticket?> GetTicketFromMember(GuildMemberKey member);

    /// <summary>
    /// Get all tickets in a guild.
    /// </summary>
    Task<IEnumerable<Ticket>> GetTicketsFromGuild(ulong id);

    /// <summary>
    /// Open a ticket properly and let the plebs in.
    /// </summary>
    Task OpenTicket(TicketOpenRequest tor);

    /// <summary>
    /// Close an open ticket
    /// </summary>
    Task CloseTicket(TicketCloseRequest req);

    /// <summary>
    /// Abort a ticket that did not get past the "questioning" phase.
    /// </summary>
    Task AbortTicket(Ticket ticket);

}
