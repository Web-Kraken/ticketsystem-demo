namespace TSModMail.Core.Entities.Tickets;

/// <summary>
/// The status of the <see cref="Ticket"/>.
/// </summary>
public enum TicketStageStatus : byte
{
    /// <summary>
    /// The ticket has been closed.
    /// </summary>
    Closed = 0,
    /// <summary>
    /// The user is being asked questioned by the menu service.
    /// </summary>
    Menu = 1,
    /// <summary>
    /// Ticket is open normally.
    /// </summary>
    Open = 2,
}
