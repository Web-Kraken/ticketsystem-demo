using MongoDB.Driver;
using TSModMail.Core.Entities.Menus;
using TSModMail.Core.Repositories;

namespace TSModMail.Repositories.MongoDb;

public class MongoModalRepository : MongoRepository<MenuModal, string>, IModalRepository
{
    public MongoModalRepository(IMongoCollection<MenuModal> col) : base(col)
    {
    }
}
