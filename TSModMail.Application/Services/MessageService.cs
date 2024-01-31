using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SLog;
using System.Threading.Tasks;
using TSModMail.Core.Repositories;
using TSModMail.Core.Services;
using TSModMail.Core.Services.Tickets;

namespace TSModMail.Application.Services;

/// <inheritdoc cref="IMessageService"/>
public class MessageService : IMessageService
{
    private readonly ITicketService _tickets;
    private readonly IAttachmentService _attachments;
    private readonly ITicketRegionRepository _config;
    private readonly IAsyncLog _log;

    public MessageService(
        ITicketService tickets,
        IAttachmentService attachments,
        ITicketRegionRepository config,
        IAsyncLog @base
        )
    {
        _tickets = tickets;
        _attachments = attachments;
        _config = config;
        _log = new PrefixedLog(@base, $"[{nameof(MessageService).ToUpper()}] ");
    }

    public async Task HandleMessage(DiscordClient client, MessageCreateEventArgs mcea)
    {
        var msg = mcea.Message;

        if (msg.Author.IsBot) return;

        if (msg.Channel is DiscordDmChannel)
        {
            //TODO: Make this configurable.
            await mcea.Message.RespondAsync($"Technical support is not offered in DMs, please open a ticket in the discord server if you require further assistance.");
            return;
        }

        if (msg.Attachments.Count < 1) return; // Not an attachment.

        var ticket = await _tickets.GetTicketFromChannel(mcea.Channel.Id);

        if (ticket == null) return; // Not a ticket.

        await _attachments.StoreAttachments(msg, ticket);
    }

}
