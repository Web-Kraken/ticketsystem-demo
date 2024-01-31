using System;

namespace TSModMail.Core.Entities;

/// <summary>
/// A record that describes a users permission level and the reason
/// for the last permission update.
/// </summary>
public class PermissionRecord : Entity<Guid>
{
    public PermissionRecord(GuildMemberKey member, Guid region) : base(Guid.NewGuid())
    {
        Member = member;
        Region = region;

        Level = BotPermissionLevel.User;
        LastUpdateReason = "User added.";
        LastUpdate = DateTime.UtcNow;
    }

    /// <summary>
    /// The user.
    /// </summary>
    public GuildMemberKey Member { get; private set; }
    /// <summary>
    /// Which region.
    /// </summary>
    public Guid Region { get; private set; }
    /// <summary>
    /// Permission level of the user.
    /// </summary>
    public BotPermissionLevel Level { get; private set; }
    /// <summary>
    /// The reason for the last permission update.
    /// </summary>
    public string LastUpdateReason { get; private set; }
    public DateTimeOffset LastUpdate { get; private set; } 

    /// <summary>
    /// Update a permission level.
    /// </summary>
    public void Update(BotPermissionLevel newPerms, string reason)
    {
        Level = newPerms;

        LastUpdate = DateTime.UtcNow;
        LastUpdateReason = reason;
    }
}
