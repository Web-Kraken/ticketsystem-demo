namespace TSModMail.Core.Entities.Tickets;

/// <summary>
/// A request to move a ticket from <see cref="TicketStageStatus.Menu"/> to 
/// <see cref="TicketStageStatus.Open"/>.
/// </summary>
public struct TicketOpenRequest
{
    /// <summary>
    /// The ticket being altered.
    /// </summary>
    public readonly Ticket Ticket;

    /// <summary>
    /// The path of the menu options that lead to this point.
    /// </summary>
    public readonly string NodePath;

    /// <summary>
    /// A message to print while opening the ticket.
    /// </summary>
    public readonly string? OpenMessage;

    public TicketOpenRequest(Ticket ticket, string nodePath, string? openMessage)
    {
        Ticket = ticket;
        NodePath = nodePath;
        OpenMessage = openMessage;
    }
}
