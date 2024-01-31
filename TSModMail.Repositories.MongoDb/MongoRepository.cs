using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using TSModMail.Core;
using TSModMail.Core.Entities;

namespace TSModMail.Repositories.MongoDb;

/// <summary>
/// A standard Mongo Repository implementation.
/// </summary>
public class MongoRepository<T, TId> : ICrudBase<T, TId> where T : Entity<TId>
{
    protected readonly IMongoCollection<T> col;
    public MongoRepository(IMongoCollection<T> col)
    {
        this.col = col;
    }

    public async Task<IEnumerable<T>> GetAll()
    {
        var items = await col.FindAsync(_ => true);
        return items.ToEnumerable();
    }

    public async Task<T?> GetById(TId oid)
    {
        var items = await col.FindAsync(t => t.Id!.Equals(oid));
        return await items.FirstOrDefaultAsync();
    }

    public async Task Insert(T item)
    {
        await col.InsertOneAsync(item);
    }

    public Task Update(T item)
    {
        return col.ReplaceOneAsync(
            obj => obj.Id!.Equals(item.Id),
            item,
            new ReplaceOptions() { IsUpsert = true });
    }

    public async Task Delete(T item)
    {
        await col.DeleteOneAsync(obj => obj.Id!.Equals(item.Id));
    }
}
