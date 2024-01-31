using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using SLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using TSModMail.Application.Helpers;
using TSModMail.Core.Repositories;
using TSModMail.Core.Services;
using TSModMail.Core.Services.Tickets;

namespace TSModMail.Bot;

internal class TicketBot
{
    private readonly IAsyncLog _log;

    private readonly DiscordClient _client;
    private readonly SlashCommandsExtension _slash;

    private readonly ICommandService _commands;
    private readonly IMenuService _menu;
    private readonly IMessageService _messages;
    private readonly ICleanupService _cleanup;
    private readonly ITicketWatchdogService _watchdog;

    private readonly ITicketRegionRepository _guilds;

    private readonly CancellationTokenSource repeatTaskCancellation = new CancellationTokenSource();

    private Task? _guildTask = null;
   

    public TicketBot(
        DiscordClient client,
        IAsyncLog @base,
        SlashCommandsExtension slash,
        ICommandService commands,
        IMenuService menu,
        IMessageService messages,
        ICleanupService cleanup,
        ITicketRegionRepository guilds,
        ITicketWatchdogService watchdog
    )
    {
        _client = client;
        _commands = commands;
        _slash = slash;

        _menu = menu;
        _messages = messages;
        _cleanup = cleanup;
        _guilds = guilds;

        _log = new PrefixedLog(@base, "[CORE] ");

        _client.SessionCreated += HandleSessionReady;
        _client.Zombied += HandleSessionZombied;

        _client.GuildAvailable += HandleGuildAvailable;
        _client.ComponentInteractionCreated += HandleComponentInteraction;

        _client.ComponentInteractionCreated += _menu.HandleComponentInteract;
        _client.ModalSubmitted += _menu.HandleModalSubmitted;
        _client.MessageCreated += _messages.HandleMessage;

        _slash.SlashCommandErrored += HandleSlashError;
        _slash.SlashCommandInvoked += HandleSlashInvoked;
        _slash.SlashCommandExecuted += HandleSlashExecuted;

        _client.UnknownEvent += HandleUnknownEvent;
        _client.ClientErrored += HandleClientError;

        AppDomain.CurrentDomain.UnhandledException +=
             (object sender, UnhandledExceptionEventArgs e)
         => _log
             .LogError("Unhandled Exception in the App Domain", (Exception)e.ExceptionObject)
             .Wait();

        _guilds = guilds;
        _watchdog = watchdog;
    }

    internal async Task StartAsync()
    {
        await _log.LogInfo("Starting.");

        await _commands.RegisterCommands();

        await _client.ConnectAsync();

        // TODO: _ = Task.Run(CleanupAttachments, repeatTaskCancellation.Token);
    }

    private async Task GuildTasks()
    {
        await Task.Delay(TimeSpan.FromMinutes(1)); // TODO: FIX THIS
        while (true)
        {
            foreach (var config in await _guilds.GetAll())
            {
                await _cleanup.CleanupQuestioningTickets(config);
                await _watchdog.RefreshTicketChannels(config);
            }
            await Task.Delay(TimeSpan.FromMinutes(5));
        }
    }

    private async Task CleanupAttachments()
    {
        await Task.Delay(TimeSpan.FromMinutes(1));
        while (true)
        {
            var guidlds = await _guilds.GetAll();
            foreach (var guild in guidlds)
            {
                await _cleanup.CleanupOldTicketLogs(guild);
                await _cleanup.CleanupOldAttachments(guild);
            }
            await Task.Delay(TimeSpan.FromHours(1));
        }
    }

    private async Task HandleSlashInvoked(SlashCommandsExtension sender, SlashCommandInvokedEventArgs e)
    {
        await _log.LogInfo($"{StringHelper.Format(e.Context.Member)} ran /{e.Context.CommandName} in {StringHelper.Format(e.Context.Channel)})");
    }

    private async Task HandleSlashError(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
    {
        await _log.LogError($"Exception in slash command: {StringHelper.Format(e.Context.Member)} ran /{e.Context.CommandName} in {StringHelper.Format(e.Context.Channel)})", e.Exception);
    }

    private async Task HandleClientError(DiscordClient sender, ClientErrorEventArgs e)
    {
        await _log.LogError($"Exception in event {e.EventName}", e.Exception);
    }

    private async Task HandleUnknownEvent(DiscordClient sender, UnknownEventArgs e)
    {
        await _log.LogInfo($"Unknown Event - {e.EventName}\n   {e.Json.Limit(100)}");
    }

    private async Task HandleGuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
    {
        await _log.LogInfo($"Guild available - {e.Guild.Name} ({e.Guild.Id})");
    }

    private async Task HandleSessionReady(DiscordClient sender, SessionReadyEventArgs e)
    {
        await _log.LogInfo($"Logged in as {sender.CurrentUser.Username} ({sender.CurrentUser.Id})");

        if (_guildTask is null)
        {
            _guildTask = Task.Run(GuildTasks, repeatTaskCancellation.Token);
        }
    }

    private async Task HandleSessionZombied(DiscordClient sender, ZombiedEventArgs args)
    {
        await _log.LogInfo($"We're zombied, send help.");
    }

    private Task HandleSlashExecuted(SlashCommandsExtension sender, SlashCommandExecutedEventArgs e)
    {
        //TODO: Add tracing / logs
        return Task.CompletedTask;
    }

    private async Task HandleComponentInteraction(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        await _log.LogDebug($"Component Interaction: {e.Id} {e.User} {e.Message}");
    }

}
