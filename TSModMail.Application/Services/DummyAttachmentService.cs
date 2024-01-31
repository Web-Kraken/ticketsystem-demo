using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TSModMail.Core.Entities;
using TSModMail.Core.Entities.Tickets;
using TSModMail.Core.Services;

namespace TSModMail.Application.Services;

public class DummyAttachmentService : IAttachmentService
{
    public Task<IEnumerable<AttachmentMap>> GetAllForGuild(ulong guildId)
    {
        return Task.FromResult(Enumerable.Empty<AttachmentMap>());
    }

    public Task<AttachmentMap?> GetMapForId(ulong id)
    {
        return Task.FromResult<AttachmentMap?>(null);
    }

    public Task StoreAttachments(DiscordMessage msg, Ticket ticket)
    {
        return Task.CompletedTask;
    }
}
