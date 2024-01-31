using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TSModMail.Core.Entities.Tickets;
using TSModMail.Core.Repositories;

namespace TSModMail.Repositories.MongoDb;

/// <inheritdoc cref="ITicketRepository"/>
public class MongoTicketRepository
    : MongoRepository<Ticket, Guid>, ITicketRepository
{
    public MongoTicketRepository(IMongoCollection<Ticket> col) : base(col)
    {
    }

    public async Task<Ticket?> GetFromChannel(ulong channelId)
    {
        var result = await col.FindAsync(ticket => ticket.ChannelId == channelId);
        return await result.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Ticket>> GetTicketsFromGuild(ulong id)
    {
        var tickets = await col.FindAsync(ticket => ticket.Author.GuildId == id);
        return await tickets.ToListAsync();
    }
}
