using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TSModMail.Core.Entities;
using TSModMail.Core.Entities.Tickets;
using TSModMail.Core.Repositories;

namespace TSModMail.Repositories.MongoDb;

/// <inheritdoc cref="ITicketHistoryRepository"/>
public class MongoTicketHistoryRepository
    : MongoRepository<HistoricTicket, Guid>, ITicketHistoryRepository
{
    public MongoTicketHistoryRepository(IMongoCollection<HistoricTicket> col)
        : base(col)
    {
    }

    public async Task<IEnumerable<HistoricTicket>> GetHistory(GuildMemberKey key)
    {
        var cursor = await col.FindAsync(doc => doc.Recipient == key);
        return await cursor.ToListAsync();
    }

    public async Task<IEnumerable<HistoricTicket>> GetHistoryRangeForUser(
        ulong guildId,
        ulong participantId,
        DateTime start,
        DateTime end
    )
    {
        var cursor = await col.FindAsync(doc =>
            doc.Recipient.GuildId == guildId &&
            doc.ClosedAt > start &&
            doc.ClosedAt < end &&
            doc.Participants.Contains(participantId)
        );
        return await cursor.ToListAsync();
    }

    public async Task<IEnumerable<HistoricTicket>> GetHistoryRangeForGuild(ulong guildId, DateTime start, DateTime end)
    {
        var cursor = await col.FindAsync(doc =>
            doc.ClosedAt > start &&
            doc.ClosedAt < end &&
            doc.Recipient.GuildId == guildId
         );
        return await cursor.ToListAsync();
    }


}
