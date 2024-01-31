using DSharpPlus.Entities;

namespace TSModMail.Core.Entities.Tickets;

/// <summary>
/// Ticket Close Request 
/// </summary>
public struct TicketCloseRequest
{
    /// <summary>
    /// The <see cref="Ticket"/> being closed.
    /// </summary>
    public readonly Ticket Ticket;

    /// <summary>
    /// The user who requested the ticket be closed.
    /// </summary>
    public readonly DiscordMember Closer;

    /// <summary>
    /// The reason 
    /// </summary>
    public readonly string Reason;

    public TicketCloseRequest(
        Ticket ticket,
        DiscordMember closer,
        string reason
    )
    {
        Ticket = ticket;
        Closer = closer;
        Reason = reason;
    }
}