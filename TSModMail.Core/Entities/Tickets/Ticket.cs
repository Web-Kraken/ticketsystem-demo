using System;
using System.Collections.Generic;

namespace TSModMail.Core.Entities.Tickets;

/// <summary>
/// Information about a ticket thread.
/// </summary>
public class Ticket : Entity<Guid>
{
    /// <summary>
    /// The "end user".
    /// </summary>
    public GuildMemberKey Author { get; private set; }

    /// <summary>
    /// When the user began <see cref="TicketStageStatus.Menu"/> the ticket.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// When was the ticket closed.
    /// </summary>
    public DateTime? ClosedAt { get; private set; }

    /// <summary>
    /// The status of the ticket.
    /// </summary>
    public TicketStageStatus Status { get; set; }

    /// <summary>
    /// The Id of the Discord channel for the ticket.
    /// </summary>
    public ulong ChannelId { get; private set; }

    /// <summary>
    /// Which region does this ticket belong to.
    /// </summary>
    public Guid CreationRegion { get; private set; }

    /// <summary>
    /// What menu node path caused this ticket creation.
    /// </summary>
    public string CreationNode { get; set; }

    public List<string> Notes { get; set; }

    /// <summary>
    /// Get an embeddable mention for the channel.
    /// </summary>
    public string Mention => $"<#{ChannelId}>";

    /// <summary>
    /// The name of the ticket.
    /// </summary>
    public string Name => $"ticket-{Id.ToString().Substring(0, 7)}";

    public Ticket(
        Guid id,
        GuildMemberKey author,
        ulong channel,
        Guid region
    )
        : base(id)
    {
        Author = author;
        ChannelId = channel;
        CreationRegion = region;

        CreatedAt = DateTime.UtcNow;
        Status = TicketStageStatus.Menu;
        Notes = new List<string>();
    }

    public void Close()
    {
        ClosedAt = DateTime.UtcNow;
        Status = TicketStageStatus.Closed;
    }
}
