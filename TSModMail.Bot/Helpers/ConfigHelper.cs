using Newtonsoft.Json;
using System;
using System.IO;
using TSModMail.Bot.Entities;

namespace TSModMail.Bot.Helpers;

/// <summary>
/// Helper for configs.
/// </summary>
internal static class ConfigHelper
{
    public const string CONFIG_PATH = @"./config.jsonc";

    internal static TicketBotConfig GetConfig()
    {
        if (!File.Exists(CONFIG_PATH))
        {
            throw new FileNotFoundException($"Could not find file {CONFIG_PATH}");
        }

        var content = File.ReadAllText(CONFIG_PATH);

        try
        {
            return
                JsonConvert.DeserializeObject<TicketBotConfig>(content)
                ?? throw new InvalidOperationException("Config was null.");
        } catch (JsonSerializationException ex) {
            throw new InvalidOperationException($"Config is invalid. {ex.Message}");
        }
    }
}
