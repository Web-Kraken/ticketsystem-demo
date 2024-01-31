namespace TSModMail.Application.Services;
//TODO: Rework the AttachmentService
#if UNUSED
/// <inheritdoc cref="IAttachmentService"/>
public class AttachmentService : IAttachmentService
{
    private readonly IAttachmentRepository _attachments;
    private readonly ITicketRegionRepository _config;
    private readonly DiscordClient _client;
    private readonly IAsyncLog _log;
    public AttachmentService(
        IAttachmentRepository attachments,
        ITicketRegionRepository config,
        DiscordClient client,
        IAsyncLog @base
        )
    {
        _attachments = attachments;
        _config = config;
        _client = client;

        _log = new PrefixedLog(@base, $"[{nameof(AttachmentService).ToUpper()}] ");
    }

    public async Task<IEnumerable<AttachmentMap>> GetAllForGuild(ulong guildId)
    {
        var atts = await _attachments.GetAll();

        return atts.Where(att => att.GuildId == guildId);
    }

    public Task<AttachmentMap?> GetMapForId(ulong id)
    {
        return _attachments.GetById(id);
    }

    public async Task StoreAttachments(DiscordMessage msg, Ticket ticket)
    {
        var config = await _config.GetById(msg.Channel.Guild.Id);

        if (config == null)
        {
            return;
        }

        if (!config.AttachmentChannelId.HasValue)
        {
            return;
        }

        var attachmentChannel = msg.Channel.Guild.GetChannel(config.AttachmentChannelId.Value);

        if (attachmentChannel == null)
        {
            await _log.LogError($"Cannot store attachments for message {msg.Id} because channel {config.AttachmentChannelId} does not exist.", null);
            return;
        }

        var tasks = msg.Attachments.Select(async attachment =>
        {

            if (attachment.FileSize > GuildHelpers.GetMaxFileSize(attachmentChannel.Guild))
            {
                await _log.LogDebug($"Attachment {attachment.Url} is too big to replicate.");
                return new AttachmentMap(attachment, attachment, msg.Channel!.GuildId!.Value);
            }

            await _log.LogDebug($"Getting attachment {attachment.Url} from {msg.Id}");
            var stream = await WebHelper.GetStreamFromUrl(attachment.Url);

            var newMessage = await attachmentChannel
                .SendMessageAsync(
                    new DiscordMessageBuilder()
                    .WithContent($"Posted in {ticket.Mention} ({ticket.Id}) by {msg.Author.Mention} ({msg.Author.Id})")
                    .AddFile(attachment.FileName, stream)
                    );


            var map = new AttachmentMap(attachment, newMessage.Attachments[0], msg!.Channel!.GuildId.Value);

            return map;
        });

        var maps = await Task.WhenAll(tasks);

        await Task.WhenAll(maps.Select(map => _attachments.Insert(map)));

        await _log.LogDebug($"Inserted {maps.Length} attachment maps into db.");
    }
}
#endif