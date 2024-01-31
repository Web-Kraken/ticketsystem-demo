using DSharpPlus.Entities;
using System;

namespace TSModMail.Core.Entities;

/// <summary>
/// Relates a saved attachment with its original.
/// </summary>
public class AttachmentMap : Entity<ulong>
{
    /// <summary>
    /// Original attachment from the ticket.
    /// </summary>
    public DiscordAttachment Old { get; set; }

    /// <summary>
    /// Reuploaded attachment.
    /// </summary>
    public DiscordAttachment New { get; set; }

    /// <summary>
    /// When the attachment was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Which guild was it posted in.
    /// </summary>
    public ulong GuildId { get; set; }

    /// <summary>
    /// The new url of the asset.
    /// </summary>
    public string NewUrl => New.Url;

    /// <summary>
    /// The snowflake of which was uploaded.
    /// </summary>
    public ulong OriginalId => Id;

    public AttachmentMap(
        DiscordAttachment old, 
        DiscordAttachment @new,
        ulong guildId
    ) : base(old.Id)
    { 
        Old = old ?? throw new ArgumentNullException(nameof(old));
        New = @new ?? throw new ArgumentNullException(nameof(@new));
        GuildId = guildId;

        CreatedAt = DateTime.UtcNow;
    }
}
