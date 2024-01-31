using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TSModMail.Core.Entities;
using TSModMail.Core.Repositories;

namespace TSModMail.Repositories.MongoDb;

/// <inheritdoc cref="IPermissionRepository"/>
public class MongoPermissionRepository
    : MongoRepository<PermissionRecord, Guid>, IPermissionRepository
{
    public MongoPermissionRepository
        (IMongoCollection<PermissionRecord> col) : base(col)
    {
    }

    public async Task<IEnumerable<PermissionRecord>> GetAllForRegion(Guid region)
    {
        var cursor = await col.FindAsync(x => x.Region == region);

        return await cursor.ToListAsync();
    }

    public async Task<IEnumerable<PermissionRecord>> GetForMember(GuildMemberKey key)
    {
        var cursor = await col.FindAsync(
            x => x.Member == key
        );

        return await cursor.ToListAsync();
    }

    public async Task<PermissionRecord?> GetForMember(GuildMemberKey key, Guid region)
    {
        var cursor = await col.FindAsync(x => x.Member == key && x.Region == region);

        return await cursor.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<PermissionRecord>> GetForRegion(Guid region)
    {
        var cursor = await col.FindAsync(x => x.Region == region);

        return await cursor.ToListAsync();
    }
}
