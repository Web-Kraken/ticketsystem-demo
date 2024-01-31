using System;

namespace TSModMail.Core.Entities;

/// <summary>
/// A Users permission level.
/// </summary>
public enum BotPermissionLevel : Int32
{
    /// <summary>
    /// A blacklisted user cannot open tickets.
    /// </summary>
    Blacklisted = 0,
    /// <summary>
    /// A normal user of the ticket bot.
    /// </summary>
    User = 1,
    /// <summary>
    /// An assistant.
    /// </summary>
    Assistant = 2,
    /// <summary>
    /// A tech support member.
    /// </summary>
    Support = 3,
    /// <summary>
    /// A tech support supervisor. 
    /// </summary>
    Supervisor = 4, // TODO: Rename.
    /// <summary>
    /// Developer access.
    /// </summary>
    Dev = 5,
}
