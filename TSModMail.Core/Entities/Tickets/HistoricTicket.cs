using System;

namespace TSModMail.Core.Entities.Tickets;

/// <summary>
/// A ticket which has been closed.
/// </summary>
public class HistoricTicket : Entity<Guid>
{
    /// <summary>
    /// Id of the closed ticket log.
    /// </summary>
    public ulong MessageId { get; private set; }

    /// <summary>
    /// Link to the log post.
    /// </summary>
    public string? MessageLink { get; private set; }

    /// <summary>
    /// Whoever opened the ticket.
    /// </summary>
    public GuildMemberKey Recipient { get; private set; }

    /// <summary>
    /// Everyone who interacted with the ticket.
    /// </summary>
    public ulong[] Participants { get; private set; }

    /// <summary>
    /// When the user began <see cref="TicketStageStatus.Questioning"/> the ticket.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// When the ticket was closed.
    /// </summary>
    public DateTime ClosedAt { get; private set; }

    /// <summary>
    /// The reason specified when the tickets were closed.
    /// </summary>
    public string Reason { get; private set; }

    /// <summary>
    /// Which region was this ticket related to.
    /// </summary>
    public Guid Region { get; private set; }

    /// <summary>
    /// Marks whether the ticket log for this log has been deleted.
    /// </summary>
    public bool IsClean { get; set; }

    public HistoricTicket(
        string? messageLink,
        GuildMemberKey recipient,
        ulong[] participants,
        DateTime createdAt,
        DateTime closedAt,
        string reason,
        Guid region,
        Guid ticketId
    ) : base(ticketId)
    {
        MessageLink = messageLink;
        Recipient = recipient;
        Participants = participants;
        CreatedAt = createdAt;
        ClosedAt = closedAt;
        Reason = reason;
        Region = region;
        IsClean = false;
    }

}
