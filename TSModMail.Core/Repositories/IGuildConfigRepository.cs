using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TSModMail.Core.Entities;

namespace TSModMail.Core.Repositories;

/// <summary>
/// Repository for <see cref="TicketRegion"/>.
/// </summary>
public interface ITicketRegionRepository : ICrudBase<TicketRegion, Guid>
{
    Task<TicketRegion?> GetByName(ulong guildId, string name);
    Task<IEnumerable<TicketRegion>> GetFromGuild(ulong guildId);
}
