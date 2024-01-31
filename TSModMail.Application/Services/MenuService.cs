using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using TSModMail.Core.Services;
using TSModMail.Application.Helpers;
using TSModMail.Core.Entities;
using TSModMail.Core.Entities.Menus;
using TSModMail.Core.Repositories;
using TSModMail.Core.Entities.Tickets;
using System.Threading.Tasks;
using System;
using System.Linq;
using SLog;
using DSharpPlus.Interactivity;
using System.Collections.Generic;
using TSModMail.Core.Services.Tickets;

namespace TSModMail.Application.Services;

/// <inheritdoc cref="IMenuService"/>
public class MenuService : IMenuService
{
    private const string NO_OPEN_DEFAULT = "We're currently experiencing a high volume of tickets, please try again in a few hours time.";

    private readonly IMenuRepository _repo;
    private readonly IPermissionService _permissions;
    private readonly ITicketService _tickets;
    private readonly IRegionService _regions;
    private readonly INotificationService _notif;
    private readonly IModalRepository _modals;
    private readonly InteractivityExtension _interactivity;
    private readonly IAsyncLog _log;

    public MenuService(
        IPermissionService permissions,
        ITicketService tickets,
        IMenuRepository repo,
        IRegionService regions,
        IModalRepository modals,
        InteractivityExtension interactivity,
        INotificationService notif,
        IAsyncLog @base
    )
    {
        _permissions = permissions;
        _tickets = tickets;
        _repo = repo;
        _modals = modals;
        _regions = regions;
        _interactivity = interactivity;
        _notif = notif;

        _log = new PrefixedLog(@base, $"[{nameof(MenuService).ToUpper()}] ");
    }

    public async Task HandleComponentInteract(
        DiscordClient sender,
        ComponentInteractionCreateEventArgs e
    )
    {
        await _log.LogDebug($"Interaction received from {e.User.Id} in {e.Guild.Id} \"{e.Id}\"");

        var member = await e.Guild.GetMemberAsync(e.User.Id);

        var id = e.Id.Split(":");
        var regionId = Guid.Parse(id.First());
        var path = id.Last();

        var region = await _regions.GetByGuid(regionId);

        if (region is null)
        {
            await e.Interaction.RespondEphemeral(
                "There is a region misconfiguration. This has been reported. Please try again later."
            );
            await _log.LogError($"Invalid region, {regionId} in {e.Guild.Id} in channel {e.Channel.Id}", null);

            return;
        }

        var permissions = await _permissions.GetForMember(member, region.Id);

        if (permissions != null && permissions.Level == BotPermissionLevel.Blacklisted)
        {
            await e.Interaction.RespondEphemeral(
                "You have been blacklisted from using this service. Contact the operator if you believe this is a mistake."
            );
            return;
        }


        var menus = await _repo.GetById(e.Guild.Id);
        if (menus == null)
        {
            await _log.LogError($"No guild menu for {e.Guild.Id}.", null);
            return;
        }

        var userTicket = await _tickets.GetTicketFromMember(member);

        var channelTicket = await _tickets.GetTicketFromChannel(e.Channel.Id);

        if (channelTicket == null) //TODO: Clean this up.
        {
            if (userTicket == null)
            {
                if (region.PanicMessage != null)
                {
                    var msg = string.IsNullOrWhiteSpace(region.PanicMessage)
                        ? NO_OPEN_DEFAULT
                        : region.PanicMessage;

                    await e.Interaction.RespondEphemeral(msg);

                    await _notif.SendNotification(region.Id, new DiscordMessageBuilder().WithContent($"Ticket denied for {e.User.Mention} ({e.User.Id}) at `{e.Id}` because of panic mode."));

                    return;
                }

                var newTicket = await BeginTicket(sender, path, member, menus, region);

                await e.Interaction.RespondEphemeral($"Ticket created in <#{newTicket.ChannelId}>.");

                return;
            }
            else
            {
                await e.Interaction.RespondEphemeral($"You have an open ticket in {userTicket.Mention}. You can only have a maximum of one ticket open at a time.");
                return;
            }
        }
        else
        {
            if (userTicket == null || userTicket.Author.UserId != channelTicket.Author.UserId)
            {
                await e.Interaction.RespondEphemeral($"This is not your ticket. This message should never appear, unless you are someone with administrator permissions.");
                return;
            }
        }

        var child = FindChild(path, menus);

        if (child.Action.Mode != MenuNodeMode.OpenTicketModal)
        {
            await e.Message.DeleteAsync();
        }
        var channel = e.Channel;


        if (child == null)
        {
            await _log.LogError($"Outdated menu state for {userTicket.Id} at {e.Id})", null);
            await e.Interaction.RespondEphemeral("This menu option is missing or broken. This has been reported. Please try again later.");
            return;
        }

        if (child.Action.Mode == MenuNodeMode.Abort)
        {
            await AbortTicket(child, member, e, channelTicket, channel, region);

            return;
        }

        if (child.Action.Mode == MenuNodeMode.OpenTicket)
        {
            await _tickets.OpenTicket(new TicketOpenRequest(channelTicket, path, child.Action.Message));
            return;
        }

        if (child.Action.Mode == MenuNodeMode.OpenTicketModal)
        {
            if (child.Action.ModalName is null)
            {
                await _log.LogError($"Missing modal name for {userTicket.Id} at {e.Id})", null);
                await e.Interaction.RespondEphemeral("This modal option is missing or broken. This has been reported. Please try again later.");
                return;
            }

            var modal = await _modals.GetById(child.Action.ModalName);

            if (modal is null)
            {
                await _log.LogError($"Missing modal config for {userTicket.Id} at {e.Id})", null);
                await e.Interaction.RespondEphemeral("This modal option is missing or broken. This has been reported. Please try again later.");
                return;
            }

            await e.Interaction.CreateResponseAsync(
                InteractionResponseType.Modal,
                modal
                    .CreateInteraction()
                    .WithCustomId(e.Id) // TODO: CHECK THIS!! PROBABLY INCORRECT FOR MODALS
            );

            await _log.LogInfo($"Created modal {modal.Id} for {e.User.Id}");

            return;
        }

        var response = CreateMessageBuilder(child, path, region);

        await channel.SendMessageAsync(response);

        return;
    }

    private async Task AbortTicket(
        MenuNode child,
        DiscordMember member,
        ComponentInteractionCreateEventArgs e,
        Ticket channelTicket,
        DiscordChannel channel,
        TicketRegion region
    )
    {
        await _notif.SendNotification(
            region.Id,
            new DiscordMessageBuilder()
            .WithContent($"Aborted ticket for {e.User.Mention} ({e.User.Id}) at `{e.Id}`.")
        );

        try
        {
            await member.SendMessageAsync(
                child.Action.Message
            );
        }
        catch (UnauthorizedException)
        {
            await channel.SendMessageAsync(
                $"{member.Mention}\nYou have closed your DMs to this server, or have blocked this Bot. " +
                $"Whatever the case, we have been unable to send you further information about the " +
                $"matter you have opened a ticket for.\n\n{child.Action.Message}\n\nThis ticket will close in 60 seconds."
            );

            _ = Task.Run(async () =>
            {
                await _log.LogDebug($"Nuking {e.Channel.Id} in 1 minute.");
                await Task.Delay(TimeSpan.FromMinutes(1));
                await _tickets.AbortTicket(channelTicket);
            });

            return;
        }

        await _tickets.AbortTicket(channelTicket);
    }

    public async Task HandleModalSubmitted(DiscordClient sender, ModalSubmitEventArgs msea)
    {
        await _log.LogDebug(
            $"Modal submitted by @{msea.Interaction.User.Id} in #{msea.Interaction.Channel.Name} as \"{msea.Interaction.Data.Name}\""
        );

        var ticket = await _tickets.GetTicketFromChannel(msea.Interaction.ChannelId);

        if (ticket == null)
        {
            return;
        }

        await SendModalResults(
            msea.Interaction.Channel,
            msea.Interaction,
            msea.Values
        );

        var msgs = msea.Interaction.Channel.GetMessagesAsync(1);

        var menus = await _repo.GetById(msea.Interaction.Guild.Id);

        var child = FindChild(msea.Interaction.Data.CustomId, menus);

        var questionMsg = await msgs.FirstAsync(msg => msg.Components.Count > 0);

        await questionMsg.DeleteAsync(); // bomb the question message

        await _tickets.OpenTicket(
            new TicketOpenRequest(
                ticket,
                msea.Interaction.Data.Name,
                child.Action.Message
            )
        );
    }

    private async Task<Ticket> BeginTicket(
        DiscordClient sender,
        string menuPath,
        DiscordMember author,
        GuildMenus menus,
        TicketRegion region
    )
    {
        var child2 = FindChild(menuPath, menus);
        var embed = CreateMessageBuilder(child2, menuPath, region);

        var tcr = new TicketCreateRequest(author, region, menuPath);


        var ticket = await _tickets.CreateTicket(tcr);

        var ticketChannel = await sender.GetChannelAsync(ticket.ChannelId);


        embed
            .WithContent($"{embed.Content}\n{author.Mention}")
            .WithAllowedMention(new UserMention(author));

        await ticketChannel.SendMessageAsync(embed);

        return ticket;
    }

    private static async Task SendModalResults(
        DiscordChannel channelTicket,
        DiscordInteraction inter,
        IReadOnlyDictionary<string, string> dict
    )
    {
        var embed = new DiscordEmbedBuilder()
            .WithAuthor(name: $"Modal Submitted: {inter.Data.CustomId}", iconUrl: inter.User.AvatarUrl);

        foreach (var kvp in dict.Take(25))
        {
            embed.AddField($"**{kvp.Key}**", kvp.Value);
        }

        await inter.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AddEmbed(embed)
        );
    }

    private static MenuNode FindChild(string path, GuildMenus menus)
    {
        var parts = path.Split('.');
        var first = parts.First();

        var menu = menus.Menus.FirstOrDefault(c => c.Id == first);

        if (menu == null)
        {
            throw new ArgumentException($"Menu \"{first}\" not found");
        }

        if (parts.Length == 1)
        {
            return menu;
        }

        return FindChild(string.Join(".", parts.Skip(1)), menu);
    }

    private static MenuNode FindChild(string path, MenuNode root)
    {
        var parts = path.Split('.');
        var first = parts.First();

        var child = root.Children.FirstOrDefault(c => c.Id == first);

        if (child == null)
        {
            throw new ArgumentNullException(
                $"Child \"{first}\" does not exist for \"{path}\""
            );
        }

        if (parts.Length == 1)
        {
            return child;
        }

        return FindChild(string.Join(".", parts.Skip(1)), child);
    }

    private static DiscordInteractionResponseBuilder BuildInteractionResponse(
        MenuNode node,
        string parentNs
    )
    {
        var response = new DiscordInteractionResponseBuilder();

        response.AsEphemeral(true);
        response.Content = node.Content;
        foreach (var n in node.Children)
        {
            response.AddComponents(
                new DiscordButtonComponent(n.Style, $"{parentNs}.{n.Id}", n.Content)
                );
        }
        return response;
    }

    private static DiscordMessageBuilder CreateMessageBuilder(
        MenuNode node,
        string parentNs,
        TicketRegion region
    )
    {
        var response = new DiscordMessageBuilder();

        response.Content = node.Content;
        foreach (var n in node.Children)
        {
            string customId = $"{region.Id}:{parentNs}.{n.Id}";
            response.AddComponents(
                new DiscordButtonComponent(n.Style, customId, n.Content)
                );
        }
        return response;
    }
}
