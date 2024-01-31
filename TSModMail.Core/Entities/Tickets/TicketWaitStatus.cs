namespace TSModMail.Core.Entities.Tickets;

/// <summary>
/// The status of the <see cref="Ticket"/>.
/// </summary>
public enum TicketWaitStatus
{
    /// <summary>
    /// Waiting on TS.
    /// </summary>
    WaitingOnTS = 0,
    /// <summary>
    /// Waiting on TS for more than configured period.
    /// </summary>
    WaitingOnTSLate = 1,
    /// <summary>
    /// Waiting on user.
    /// </summary>
    WaitingOnUser = 2,
    /// <summary>
    /// Waiting on user for more than configured period.
    /// </summary>
    WaitingOnUserLate = 3,
}
