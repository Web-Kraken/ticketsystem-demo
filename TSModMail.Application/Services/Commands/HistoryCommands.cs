using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TSModMail.Application.Helpers;
using TSModMail.Core.Entities;
using TSModMail.Core.Entities.Tickets;
using TSModMail.Core.Services;
using TSModMail.Core.Services.Tickets;

namespace TSModMail.Application.Services.Commands;

[SlashCommandGroup("history", "Ticket history commands")]
public class HistoryCommands : ApplicationCommandModule
{
    private readonly ITicketHistoryService _history;
    private readonly IPermissionService _permissions;
    private readonly IRegionService _regions;

    public HistoryCommands(
        ITicketHistoryService history,
        IPermissionService permissions,
        IRegionService region
    )
    {
        _history = history;
        _permissions = permissions;
        _regions = region;
    }

    [SlashCommand("user", "Get the ticket history for a user.")]
    public async Task GetUserTicketHistory(
        InteractionContext ctx,
        [Option("user", "User in question.")] DiscordUser user,
        [Option("region", "Which region.")] string? regionName = null
    )
    {
        await ctx.DeferAsync();

        var region = await _regions.TryGetRegion(regionName, ctx, BotPermissionLevel.Assistant, _permissions);

        if (region is null)
        {
            return;
        }

        var member = await ctx.Guild.GetMemberAsync(user.Id);
        var history = await _history.GetHistoricTicketsForUser(member);

        var embed = CreateHistoryListEmbed(history);
        embed.WithTitle($"History for {user.Username}.");

        await ctx.Interaction.EditOriginalResponseAsync(
            new DiscordWebhookBuilder().AddEmbed(embed)
        );
    }

    //[SlashCommand("participated", "Check activity of specified ticket staff")]
    //public async Task GetTicketHistory(
    //    InteractionContext ctx,
    //    [Option("participant", "The participant.")] DiscordUser user
    //)
    //{
    //    await ctx.DeferAsync();

    //    var perms = await _permissions.GetForUser(ctx.Member);
    //    if (perms.Level < BotPermissionLevel.Supervisor)
    //    {
    //        await ctx.EditResponsePlain("You do not have permission to execute this command.m");
    //        return;
    //    }

    //    var history = await _history.GetHistoryRangeForUser(
    //        ctx.Guild.Id,
    //        user.Id,
    //        DateTime.UtcNow - TimeSpan.FromDays(30),
    //        DateTime.UtcNow
    //    );

    //    var embed = CreateHistoryListEmbed(history);
    //    embed.WithTitle($"{user.Username.LimitAscii()} participated in the past 30 days.");

    //    await ctx.Interaction.EditOriginalResponseAsync(
    //        new DiscordWebhookBuilder().AddEmbed(embed)
    //    );
    //}

    [SlashCommand("fortnight", "Get the count of tickets closed in the last two weeks.")]
    public async Task GetFortnightCount(InteractionContext ctx)
    {
        await ctx.DeferAsync();

        var history = await _history.GetHistoryRange(
            ctx.Guild.Id,
            DateTime.UtcNow - TimeSpan.FromDays(14),
            DateTime.UtcNow
        );

        await ctx.EditResponsePlain($"There was {history.Count()} tickets closed in the past two weeks.");
    }

    //[SlashCommand("activity", "Get an activity report on all users with permissions.")]
    //public async Task GetActivityReport(InteractionContext ctx)
    //{
    //    await ctx.DeferAsync();

    //    var perms = await _permissions.GetForUser(ctx.Member);
    //    if (perms.Level < BotPermissionLevel.Supervisor)
    //    {
    //        await ctx.EditResponsePlain("You do not have permission to execute this command.");
    //        return;
    //    }

    //    var guild = ctx.Guild;

    //    var permissions =
    //        (await _permissions.GetForGuild(guild))
    //        .ToList()
    //        .Where(perms => perms.Level > BotPermissionLevel.User);

    //    var history = await _history.GetHistoryRange(
    //        guild.Id,
    //        DateTime.UtcNow - TimeSpan.FromDays(30),
    //        DateTime.UtcNow
    //    );
    //    var nameTasks = permissions.ToDictionary(
    //        kvp => kvp.Id.UserId,
    //        x => guild.GetMemberAsync(x.Id.UserId)
    //    );

    //    // Let all of the member names fetch, should be cached locally anyway.
    //    await Task.WhenAll(nameTasks.Values);

    //    var names = nameTasks.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Result.Username);

    //    var scores = permissions.ToDictionary(
    //        kvp => names[kvp.Id.UserId],
    //        kvp => history.Count(x => x.Participants.Contains(kvp.Id.UserId))
    //    );

    //    //TODO: Make this into a nice pretty embed and remove duplicates.

    //    var activity = new StringStream();

    //    const string separator = "####################";
    //    activity.WriteString(separator);
    //    activity.WriteString($"# {guild.Name} ({guild.Id})");
    //    activity.WriteString(separator);

    //    foreach (var (name, count) in scores)
    //    {
    //        activity.WriteString($"{name.PadRight(32)} - {count}");
    //    }

    //    await ctx.Channel.SendMessageAsync(mb => mb.AddFile($"{DateTime.UtcNow.ToString("d")}_activity.txt", activity));

    //    await ctx.EditResponsePlain("Done.");
    //}

    private static DiscordEmbedBuilder CreateHistoryListEmbed(IEnumerable<HistoricTicket> history)
    {
        var sorted = history.OrderByDescending(t => t.ClosedAt);

        //TODO: Paginate, and collapse duplicate days.

        var embed = new DiscordEmbedBuilder();

        embed.WithFooter($"Total: {sorted.Count()}.");

        var items = sorted.Take(25);

        if (!items.Any())
        {
            embed.AddField($"**__No recent tickets found__**", "for this user");
            return embed;
        }

        // Discord limits the overall size of an embed payload to 6k chars, adding some breathing room for footers and titles
        const int MAX_PAYLOAD = 5500;

        int fieldLimit = MAX_PAYLOAD / items.Count();

        foreach (var t in items)
        {
            var link = t.MessageLink is null ? t.Id.ToString() : $"[{t.Id}]({t.MessageLink})";
            var date = $"**__{t.ClosedAt:r}__**";

            // Discord limits to 1024 chars for an embed value, also to fit within payload max.
            embed.AddField(date, $" \"{t.Reason.Limit(fieldLimit - (link.Length + date.Length) - 1)}\" {link}");
        }

        return embed;
    }
}
