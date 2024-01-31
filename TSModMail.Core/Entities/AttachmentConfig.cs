using System;

namespace TSModMail.Core.Entities;

/// <summary>
/// Ticket attatchment policy.
/// </summary>
public class AttachmentConfig
{
    /// <summary>
    /// Discord channel for logging.
    /// </summary>
    public ulong ChannelId { get; set; }

    /// <summary>
    /// How long to keep them for in seconds.
    /// </summary>
    public ulong RetentionLength { get; set; } = (ulong)TimeSpan.FromDays(30).TotalSeconds;
}