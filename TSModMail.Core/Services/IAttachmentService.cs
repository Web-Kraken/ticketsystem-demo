using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using TSModMail.Core.Entities;
using TSModMail.Core.Entities.Tickets;

namespace TSModMail.Core.Services;

/// <summary>
/// Service that handles saving and updating attachments.
/// </summary>
public interface IAttachmentService
{
    /// <summary>
    /// Store a Discord attachment that was posted in a given ticket.
    /// </summary>
    Task StoreAttachments(DiscordMessage msg, Ticket ticket);

    /// <summary>
    /// Get an attachment for a posted message id.
    /// </summary>
    Task<AttachmentMap?> GetMapForId(ulong id);

    /// <summary>
    /// Get all attachment maps in a guild.
    /// </summary>
    Task<IEnumerable<AttachmentMap>> GetAllForGuild(
        ulong guildId
    );
}
