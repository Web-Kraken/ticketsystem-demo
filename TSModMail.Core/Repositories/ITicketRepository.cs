using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TSModMail.Core.Entities.Tickets;

namespace TSModMail.Core.Repositories;

/// <summary>
/// Repository for <see cref="Ticket"/>s.
/// </summary>
public interface ITicketRepository : ICrudBase<Ticket, Guid>
{
    Task<Ticket?> GetFromChannel(ulong channelId);
    Task<IEnumerable<Ticket>> GetTicketsFromGuild(ulong id);
}
