using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TSModMail.Core.Entities;
using TSModMail.Core.Repositories;

namespace TSModMail.Repositories.MongoDb;

/// <inheritdoc cref="ITicketRegionRepository"/>
public class MongoRegionRepository
    : MongoRepository<TicketRegion, Guid>, ITicketRegionRepository
{
    public MongoRegionRepository(
        IMongoCollection<TicketRegion> col
    ) : base(col)
    {
    }

    public async Task<TicketRegion?> GetByName(ulong guildId, string name)
    {
        var cursor = await col.FindAsync(
            x => x.GuildId == guildId && x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)
        );

        return await cursor.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TicketRegion>> GetFromGuild(ulong guildId)
    {
        var cursor = await col.FindAsync(x => x.GuildId == guildId);

        return await cursor.ToListAsync();
    }
}
