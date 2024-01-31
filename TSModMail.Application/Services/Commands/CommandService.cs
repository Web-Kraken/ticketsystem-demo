using DSharpPlus;
using DSharpPlus.SlashCommands;
using SLog;
using System.Threading.Tasks;
using TSModMail.Core.Repositories;
using TSModMail.Core.Services;

namespace TSModMail.Application.Services.Commands;

/// <inheritdoc cref="ICommandService"/>
public class CommandService : ICommandService
{
    private readonly DiscordClient _client;
    private readonly SlashCommandsExtension _slash;
    private readonly ITicketRegionRepository _guilds;
    private readonly IAsyncLog _log;
    public CommandService(
        DiscordClient client,
        SlashCommandsExtension slash,
        ITicketRegionRepository guilds,
        IAsyncLog logBase
    )
    {
        _client = client;
        _slash = slash;
        _log = new PrefixedLog(logBase, $"[{nameof(CommandService)}] ");
        _guilds = guilds;
    }

    public async Task RegisterCommands()
    {
        await _log.LogInfo("Registering commands.");

        _slash.RegisterCommands<HistoryCommands>();
        _slash.RegisterCommands<PermissionCommands>();
        _slash.RegisterCommands<RegionCommands>();
        _slash.RegisterCommands<TicketCommands>();
    }
}
