using DSharpPlus;
using DSharpPlus.Entities;
using SLog;
using System;
using System.Threading.Tasks;
using TSModMail.Core.Repositories;
using TSModMail.Core.Services;

///<inheritdoc cref="INotificationService"/>
namespace TSModMail.Application.Services;

public class DiscordNotificationService : INotificationService
{
    private readonly DiscordClient _client;
    private readonly ITicketRegionRepository _regions;
    private readonly IAsyncLog _log;

    public DiscordNotificationService(
        DiscordClient client,
        ITicketRegionRepository region,
        IAsyncLog @base
    )
    {
        _client = client;
        _regions = region;
        _log = new PrefixedLog(@base, $"[NOTIFICATIONS] ");
    }

    public async Task SendNotification(Guid regionName, DiscordMessageBuilder msg)
    {
        var config = await _regions.GetById(regionName);

        if (config == null)
        {
            await _log.LogWarning($"Trying to send notification to region {regionName} which has no config.");
            return;
        }

        if (!config.LogChannelId.HasValue)
        {
            return; // Logs are disabled.
        }

        var channel = await _client.GetChannelAsync(config.LogChannelId.Value);

        try
        {
            await channel.SendMessageAsync(msg);
        }
        catch (Exception e)
        {
            await _log.LogError($"Could not send notification to {regionName}", e);
        }
    }
}
