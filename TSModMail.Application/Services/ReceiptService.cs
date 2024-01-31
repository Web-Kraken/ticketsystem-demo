using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using SLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TSModMail.Core.Entities;
using TSModMail.Core.Entities.Tickets;
using TSModMail.Core.Services;

namespace TSModMail.Application.Services;

/// <inheritdoc cref="IReceiptService"/>
public class ReceiptService : IReceiptService
{
    private static Regex MentionRegex = new Regex("<@!*[0-9]+>");
    private static Regex RoleRegex = new Regex("<@&[0-9]+>");
    private static Regex ChannelRegex = new Regex("<#[0-9]+>");

    private readonly DiscordClient _client;
    private readonly IAttachmentService _attachments;
    private readonly IAsyncLog _log;

    public ReceiptService(
        DiscordClient client,
        IAttachmentService attachments,
        IAsyncLog @base
        )
    {
        _client = client;
        _attachments = attachments;
        _log = new PrefixedLog(@base, $"[{nameof(ReceiptService).ToUpper()}] ");
    }

    public async Task<Stream> GenerateReceipt(
        TicketCloseRequest tcr,
        IEnumerable<DiscordMessage> messages,
        DiscordChannel channel
    )
    {
        var ticket = tcr.Ticket;

        var participants = messages.Select(m => m.Author).Distinct();

        var receipt = new MemoryStream();

        WriteHeader(tcr, participants, receipt);

        foreach (var message in messages.OrderBy(m => m.Timestamp))
        {
            var att = await GetUpdatedAttachmentsAsync(message);

            WriteLine(receipt, await FormatMessageAsync(message, att, participants, channel));
        }

        WriteLine(receipt);
        WriteLine(receipt, $"Generated at {DateTime.UtcNow}");

        receipt.Seek(0, SeekOrigin.Begin);

        await _log.LogDebug($"Done generating receipt");

        return receipt;
    }

    private async Task<IEnumerable<AttachmentMap>> GetUpdatedAttachmentsAsync(DiscordMessage msg)
    {
        if (msg.Attachments.Count == 0)
        {
            return Enumerable.Empty<AttachmentMap>();
        }

        var atts = await Task.WhenAll(msg.Attachments.Select(att => _attachments.GetMapForId(att.Id)));

        return (IEnumerable<AttachmentMap>)atts.Where(a => a != null);
    }

    private async Task<string> FormatMessageAsync(
        DiscordMessage msg,
        IEnumerable<AttachmentMap> att,
        IEnumerable<DiscordUser> participants,
        DiscordChannel channel
    )
    {
        var sb = new StringBuilder();

        var content = msg.Content;

        content = await HumaniseMention(content);
        content = HumaniseRoleMention(content, channel);
        content = await HumaniseChannelMention(content);

        sb.Append($"[{msg.CreationTimestamp}] {msg.Author.Username} : {content}");
        foreach (var attachment in att)
        {
            sb.Append($"\n - {attachment.Old.FileName} {attachment.NewUrl}");
        }

        return sb.ToString();
    }

    private async Task<string> HumaniseMention(string content)
    {
        var mentions = MentionRegex.Matches(content);
        foreach (var match in mentions.ToList())
        {
            ulong memberId;
            try
            {
                memberId = ExtractId(match.Value);
            }
            catch (Exception)
            {
                continue;
            }

            var username = "unknown-user";
            try
            {
                var member = await _client.GetUserAsync(memberId);
                username = member.Username;
            }
            catch (NotFoundException) { }

            content = content.Replace(match.Value, $"@{username}");
        }

        return content;
    }

    private async Task<string> HumaniseChannelMention(string content)
    {
        var mentions = ChannelRegex.Matches(content);
        foreach (var match in mentions.ToList())
        {
            ulong roleId;
            try
            {
                roleId = ExtractId(match.Value);
            }
            catch (Exception)
            {
                continue;
            }

            var channel = "unknown-channel";
            try
            {
                var member = await _client.GetChannelAsync(roleId);
                channel = member.Name;
            }
            catch (NotFoundException) { }

            content = content.Replace(match.Value, $"#{channel}");
        }

        return content;
    }

    private string HumaniseRoleMention(string content, DiscordChannel channel)
    {
        var mentions = RoleRegex.Matches(content);
        foreach (var match in mentions.ToList())
        {
            ulong roleId;
            try
            {
                roleId = ExtractId(match.Value);
            }
            catch (Exception)
            {
                continue;
            }

            var roleName = "unknown-role";
            try
            {
                var role = channel.Guild.GetRole(roleId);
                roleName = role.Name;
            }
            catch (NotFoundException) { }

            content = content.Replace(match.Value, $"@{roleName}");
        }

        return content;
    }

    private static ulong ExtractId(string value)
    {
        int start = -1;
        int end = value.Length;

        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (start == -1 && char.IsDigit(c))
            {
                start = i;
            }
            if (start != -1 && !char.IsDigit(c))
            {
                end = i;
                break;
            }
        }

        return ulong.Parse(value.Substring(start, end - start));
    }

    private static void WriteHeader(
        TicketCloseRequest tcr,
        IEnumerable<DiscordUser> participants,
        MemoryStream receipt
    )
    {
        var ticket = tcr.Ticket;

        const string header = @"***** TICKET BOT RECEIPT *****";
        const string footer = @"******************************";

        WriteLine(receipt, header);
        WriteLine(receipt, "* ");
        WriteLine(receipt, $"* Ticket ID: {ticket.Id}");
        WriteLine(receipt, $"* Created: {ticket.CreatedAt}");
        WriteLine(receipt, $"* Closed: {ticket.ClosedAt}");
        WriteLine(receipt, $"* Closed by: {tcr.Closer.Username} ({tcr.Closer.Id})");
        WriteLine(receipt, $"* Close reason: \"{tcr.Reason.Replace('"', '\'')}\"");
        WriteLine(receipt, $"* Participants: ");
        foreach (var participant in participants)
        {
            WriteLine(receipt, $"*  > {participant.Username}#{participant.Discriminator} ({participant.Id})");
        }
        WriteLine(receipt, "* ");
        WriteLine(receipt, footer);
        WriteLine(receipt);
    }

    private static void WriteLine(MemoryStream receipt, string v)
    {
        receipt.Write(Encoding.UTF8.GetBytes($"{v}\n"));
    }

    private static void WriteLine(MemoryStream receipt)
    {
        receipt.Write(Encoding.UTF8.GetBytes("\n"));
    }
}
