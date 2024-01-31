using System.Collections.Generic;

namespace TSModMail.Core.Entities.Menus;

//TODO: Why do menus need to be guild specific.

/// <summary>
/// The list of menus available in a guild.
/// </summary>
public class GuildMenus : Entity<ulong>
{
    /// <summary>
    /// Guild this Menu belongs to.
    /// </summary>
    public ulong GuildId => Id;

    public List<MenuNode> Menus { get; private set; }

    public GuildMenus(ulong gid) : base(gid)
    {
        Menus = new List<MenuNode>();
    }

    /// <summary>
    /// Fluently add a menu child.
    /// </summary>
    public GuildMenus WithMenu(MenuNode node)
    {
        Menus.Add(node);
        return this;
    }
}
