using DSharpPlus.Entities;
using System.Threading.Tasks;
using TSModMail.Core.Entities.Tickets;

namespace TSModMail.Core.Services.Tickets;

/// <summary>
/// A service that monitors and updates Discord channel names to reflect state.
/// </summary>
public interface ITicketWatchdogService
{
    /// <summary>
    /// Handle a message in a ticket
    /// </summary>
    Task HandleTicketMessage(DiscordMessage m, Ticket t);

    /// <summary>
    /// Handle a new ticket opening.
    /// </summary>
    Task HandleTicketOpen(Ticket ticket);

    /// <summary>
    /// Refresh ticket channel names.
    /// </summary>
    Task RefreshTicketChannels(Entities.TicketRegion g);

}

