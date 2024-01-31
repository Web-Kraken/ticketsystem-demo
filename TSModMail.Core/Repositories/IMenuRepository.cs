using TSModMail.Core.Entities.Menus;

namespace TSModMail.Core.Repositories;

/// <summary>
/// Repository for <see cref="GuildMenus"/>.
/// </summary>
public interface IMenuRepository : ICrudBase<GuildMenus, ulong>
{
}
