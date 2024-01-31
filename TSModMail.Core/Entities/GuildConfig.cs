using System;
using System.Collections.Generic;

namespace TSModMail.Core.Entities;

/// <summary>
/// The elements that define a configuration for a given guild.
/// </summary>
public class TicketRegion : Entity<Guid>
{
    public TicketRegion(
        string name,
        ulong guildId,
        ulong ticketCategoryId
    )
        : base(Guid.NewGuid())
    {
        Name = name;
        GuildId = guildId;
        TicketCategoryId = ticketCategoryId;

        TicketPermissions = new List<TicketPermissions>();
    }

    /// <summary>
    /// A human legible nickname for a region.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Discord Guild Id
    /// </summary>
    public ulong GuildId { get; set; }
    /// <summary>
    /// The category where new tickets should be created.
    /// </summary>
    public ulong TicketCategoryId { get; set; }
    /// <summary>
    /// Permissions to be assigned when "opening" a new ticket.
    /// </summary>
    public List<TicketPermissions> TicketPermissions { get; set; }
    /// <summary>
    /// Category ID for the backlogged tickets.
    /// </summary>
    public ulong? BacklogCategoryId { get; set; } = null;
    /// <summary>
    /// Which channel to dump attachments into.
    /// </summary>
    public AttachmentConfig? AttachmentConfig { get; set; } = null;
    /// <summary>
    /// Where Log go.
    /// </summary>
    public ulong? LogChannelId { get; set; } = null;
    /// <summary>
    /// Sets a panic message, if non-null will deny new tickets with the message.
    /// </summary>
    public string? PanicMessage { get; set; } = null;

}
