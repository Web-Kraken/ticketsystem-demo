using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using SLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TSModMail.Application.Entities;
using TSModMail.Application.Helpers;
using TSModMail.Core.Entities;
using TSModMail.Core.Entities.Tickets;
using TSModMail.Core.Repositories;
using TSModMail.Core.Services;
using TSModMail.Core.Services.Tickets;
using static TSModMail.Core.Services.Tickets.ITicketService;

namespace TSModMail.Application.Services.Tickets;

///<inheritdoc cref="ITicketService"/>
public class TicketService : ITicketService
{
    private const int MAX_MESSAGE_LIMIT = 2000;

    private readonly ITicketRepository _tickets;
    private readonly ITicketHistoryService _history;
    private readonly INotificationService _notifs;
    private readonly DiscordClient _discord;
    private readonly IReceiptService _receipt;
    private readonly IRegionService _regions;
    private readonly IAsyncLog _log;

    public OpenTicketHandler OnOpenTicket { get; set; } = (_) => { return Task.CompletedTask; };

    public TicketService(
        ITicketRepository tickets,
        DiscordClient client,
        ITicketRegionRepository config,
        IReceiptService receipt,
        ITicketHistoryService history,
        INotificationService notifs,
        IRegionService regions,
        IAsyncLog @base
    )
    {
        _tickets = tickets;
        _discord = client;
        _receipt = receipt;
        _history = history;
        _notifs = notifs;
        _regions = regions;

        _log = new PrefixedLog(@base, $"[{nameof(TicketService).ToUpper()}] ");
    }


    private static Task<DiscordEmbedBuilder> GenerateCloseEmbed(
        TicketCloseRequest tcr
    )
    {
        var who = tcr.Closer;
        var embed = new DiscordEmbedBuilder()
            .WithTitle("Ticket Receipt")
            .AddField("Ticket Id: ", tcr.Ticket.Id.ToString(), true)
            .AddField("Created: ", $"<t:{tcr.Ticket.CreatedAt.ToUnixTimeSeconds()}:F>", true)
            .AddField("Closed: ", $"<t:{tcr.Ticket.ClosedAt!.Value.ToUnixTimeSeconds()}:F>", true)
            .AddField("Closed By: ", $"{who.Mention} ({who.Id})", true)
            .AddField("Reason: ", tcr.Reason.Limit(500), true)
            .AddField("Ticketer: ", $"<@{tcr.Ticket.Author.UserId}>", true);

        return Task.FromResult(embed);
    }

    public async Task<Ticket> CreateTicket(TicketCreateRequest tcr)
    {
        /**
         * Get category
         * Create channel 
         * setup default perms
         * add user to channel
         * tell menu service to ask questions
         */

        var guild = await _discord.GetGuildAsync(tcr.Region.GuildId);

        var me = await guild.GetMemberAsync(_discord.CurrentUser.Id);

        var overwrites = new List<DiscordOverwriteBuilder>() {
            new DiscordOverwriteBuilder(guild.EveryoneRole)
                .Deny(Permissions.AccessChannels),
            new DiscordOverwriteBuilder(me)
                .Allow(Permissions.ManageChannels | Permissions.AccessChannels),
            new DiscordOverwriteBuilder(tcr.Member)
                .Allow(Permissions.AccessChannels)
                .Deny(Permissions.SendMessages)
                .Deny(Permissions.AddReactions)
            };


        var category = guild.GetChannel(tcr.Region.TicketCategoryId);

        if (category is null)
        {
            throw new TicketServiceException(
                $"The defined category ({tcr.Region.TicketCategoryId}) does not exist."
            );
        }

        await _log.LogInfo($"Opening ticket for {tcr.Member.Username} ({tcr.Member.Id}) in {guild.Id}");

        var id = Guid.NewGuid();

        var name = $"menu-{id.ToString().Limit(6)}";

        DiscordChannel channel;
        try
        {
            await _log.LogDebug($"Trying to create channel \"{id}\" in {guild.Id}");
            channel = await guild.CreateTextChannelAsync(name, category, $"Ticket for {tcr.Member.Username}", overwrites, false);
        }
        catch (UnauthorizedException ue)
        {
            await _log.LogError($"Could not create ticket.", ue);
            throw new TicketServiceException($"Could not create ticket channel (permissions).", ue);
        }

        await _log.LogDebug($"Creating ticket entity for {tcr.Member.Id}.");

        var ticket = new Ticket(id, tcr.Member, channel.Id, tcr.Region.Id);

        await _tickets.Insert(ticket);

        return ticket;
    }

    public async Task OpenTicket(TicketOpenRequest tor)
    {
        var ticket = tor.Ticket;

        ticket.Status = TicketStageStatus.Open;

        //TODO: Change it to exceptions probs lol.
        var guild = await _discord.GetGuildAsync(ticket.Author.GuildId);

        if (guild is null)
        {
            await _log.LogError($"Cannot open ticket for {tor.Ticket.Id} because guild config is missing.", null);
            return;
        }

        var channel = guild.GetChannel(ticket.ChannelId);

        var user = await guild.GetMemberAsync(ticket.Author.UserId);

        var me = await guild.GetMemberAsync(_discord.CurrentUser.Id);

        var region = await _regions.GetByGuid(ticket.CreationRegion);

        var overwrites = new List<DiscordOverwriteBuilder>() {
            new DiscordOverwriteBuilder(me)
            .Allow(
                Permissions.ManageChannels |
                Permissions.AccessChannels |
                Permissions.SendMessages |
                Permissions.EmbedLinks |
                Permissions.AttachFiles |
                Permissions.AddReactions
            ), // 268487760
            new DiscordOverwriteBuilder(user)
            .Allow(
                Permissions.AccessChannels |
                Permissions.SendMessages |
                Permissions.EmbedLinks |
                Permissions.AttachFiles |
                Permissions.AddReactions
            ) // 52288
        };

        foreach (var perm in region!.TicketPermissions)
        {
            DiscordRole role;
            try
            {
                role = guild.GetRole(perm.RoleId);
            }
            catch (NotFoundException)
            {
                await _log.LogWarning($"Cannot find role id {perm.RoleId} in {guild.Id}");
                continue;
            }

            overwrites.Add(
                new DiscordOverwriteBuilder(role)
                .Allow(perm.Allow)
                .Deny(perm.Deny)
            );
        }

        await _log.LogDebug($"Opening the channel for ticket {ticket.ChannelId}");

        try
        {
            await channel.ModifyAsync(cem =>
            {
                cem.PermissionOverwrites = overwrites;
                cem.Name = ticket.Name;
            });
        }
        catch (UnauthorizedException e)
        {
            await _log.LogError("Could not modify the channel overwrites, likely missing ManageRoles perm.", e);
            await channel.SendMessageAsync($"A permission error has occured, this ticket cannot be created. This has been reported. Please try again later.");
            return;
        }

        try
        {
            await channel.SendMessageAsync($"Ticket opened at <t:{DateTime.UtcNow.ToUnixTimeSeconds()}:f>. {user.Mention}.");
        }
        catch (UnauthorizedException e)
        {
            await _log.LogError($"Could not post opening message in {channel.Mention}.", e);
        }

        if (tor.OpenMessage != null)
        {
            await channel.SendMessageAsync(tor.OpenMessage);
        }

        await _log.LogInfo($"Opened ticket {ticket.Id}.");

        await _notifs.SendNotification(region.Id, new DiscordMessageBuilder().WithContent($"Ticket opened in {ticket.Mention} for {user.Mention} `{tor.NodePath}`."));

        await _tickets.Update(ticket);

        await OnOpenTicket(ticket);
    }

    public async Task CloseTicket(TicketCloseRequest tcr)
    {
        var ticket = tcr.Ticket;

        var ticketChannel = await _discord.GetChannelAsync(ticket.ChannelId);

        if (ticket.Status != TicketStageStatus.Open)
        {
            try
            {
                await ticketChannel.DeleteAsync();
            } 
            catch (UnauthorizedException e)
            {
                await _log.LogError($"Could not delete channel {ticketChannel.Mention}", e);
                return; 
            }
            await _tickets.Delete(ticket);
            return;
        }

        ticket.Close();

        await _log.LogDebug($"Getting messages in {ticketChannel.Name}");
        var messageTasks = ticketChannel.GetMessagesAsync(
            MAX_MESSAGE_LIMIT
        );

        var messages = await messageTasks.ToListAsync();

        Stream receiptFile = await _receipt.GenerateReceipt(tcr, messages, ticketChannel);

        var participants = messages.Select(m => m.Author).Distinct();

        var region = await _regions.GetByGuid(ticket.CreationRegion);
        if (region is null)
        {
            await _log.LogWarning($"Orphaned ticket, {ticket.Id} in {ticketChannel.GuildId}");
            return;
        }

        var embed = await GenerateCloseEmbed(tcr);

        var messageBuilder = new DiscordMessageBuilder()
                .WithEmbed(embed)
                .AddFile("receipt.txt", receiptFile);

        DiscordMessage? logMsg = null;
        if (region.LogChannelId is not null)
        {
            var logChannel = await _discord.GetChannelAsync(region.LogChannelId.Value);

            logMsg = await logChannel.SendMessageAsync(
                messageBuilder
            );
        }

        // Reset stream to beginning 
        receiptFile.Seek(0, SeekOrigin.Begin);
        try
        {
            await ticketChannel.DeleteAsync($"Ticket closed - {tcr.Reason}.");
        } catch (UnauthorizedException e)
        {
            await _log.LogError($"Could not delete channel {ticketChannel.Mention}", e);
            return;
        }

        await _tickets.Delete(ticket); // Only delete ticket from DB if channel delete is OK

        try
        {
            var member = await ticketChannel.Guild.GetMemberAsync(ticket.Author.UserId);
            await member.SendMessageAsync(messageBuilder);
        }
        catch (Exception)
        {
            await _log.LogInfo($"Could not send receipt to {ticket.Author.UserId} (Did they leave the server?)");
        }

        await _log.LogInfo($"Closed ticket {ticket.Id}.");

        var historic = new HistoricTicket(
            logMsg?.JumpLink.ToString(),
            ticket.Author,
            participants.Select(u => u.Id).ToArray(),
            ticket.CreatedAt,
            ticket.ClosedAt!.Value,
            tcr.Reason,
            ticket.CreationRegion,
            ticket.Id
        );

        await _history.LogHistoricTicket(historic);
    }

    public async Task<Ticket?> GetTicketFromChannel(ulong channelId)
    {
        await _log.LogDebug($"Fetching ticket with channel id {channelId}.");
        return await _tickets.GetFromChannel(channelId);
    }

    public async Task<Ticket?> GetTicketById(Guid guid)
    {
        await _log.LogDebug($"Fetching ticket for {guid}.");
        return await _tickets.GetById(guid);
    }

    public async Task<Ticket?> GetTicketFromMember(GuildMemberKey key)
    {
        await _log.LogDebug($"Fetching ticket for {key}.");
        //TODO: Unsure if this is used or if it works
        return 
            (await _tickets.GetAll())
            .Where(ticket => ticket.Author == key)
            .FirstOrDefault(); 
    }

    public async Task AbortTicket(Ticket ticket)
    {
        await _log.LogDebug($"Aborting ticket {ticket.Id}.");

        var channel = await _discord.GetChannelAsync(ticket.ChannelId);
        await channel.DeleteAsync($"Aborting ticket {ticket.Id}");

        await _tickets.Delete(ticket);
    }

    public async Task<IEnumerable<Ticket>> GetAll()
    {
        await _log.LogDebug($"Getting all tickets.");

        return await _tickets.GetAll();
    }

    public async Task<IEnumerable<Ticket>> GetTicketsFromGuild(ulong guildId)
    {
        await _log.LogDebug($"Getting all tickets for guild {guildId}.");

        return await _tickets.GetTicketsFromGuild(guildId);
    }
}
