using DSharpPlus;

namespace TSModMail.Core.Entities;

/// <summary>
/// Relationship between a Discord role and its Discord permissions when 
/// creating a ticket.
/// </summary>
public class TicketPermissions : Entity<ulong>
{
    /// <summary>
    /// Discord Role Id
    /// </summary>
    public ulong RoleId => Id;

    /// <summary>
    /// Permissions to explicitly grant.
    /// </summary>
    public Permissions Allow { get; set; }

    /// <summary>
    /// Permissions to explicitly deny.
    /// </summary>
    public Permissions Deny { get; set; }

    public TicketPermissions(ulong id) : base(id)
    {
    }
}
