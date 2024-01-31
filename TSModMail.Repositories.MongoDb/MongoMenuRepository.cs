using MongoDB.Driver;
using TSModMail.Core.Entities.Menus;
using TSModMail.Core.Repositories;

namespace TSModMail.Repositories.MongoDb;

/// <inheritdoc cref="IMenuRepository"/>
public class MongoMenuRepository
    : MongoRepository<GuildMenus, ulong>, IMenuRepository
{
    public MongoMenuRepository(IMongoCollection<GuildMenus> col) : base(col)
    {
    }
}
