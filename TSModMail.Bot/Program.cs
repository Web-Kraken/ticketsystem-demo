using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using SLog;
using SLog.Discord;
using System;
using System.Threading;
using System.Threading.Tasks;
using TSModMail.Bot.Helpers;
using TSModMail.Application.Helpers;
using TSModMail.Repositories.MongoDb.Helpers;
using Sentry;
using TSModMail.Bot.Entities;

namespace TSModMail.Bot;

internal class Program
{
    private const DiscordIntents REQUIRED_INTENTS =
        DiscordIntents.AllUnprivileged |
        DiscordIntents.GuildMembers |
        DiscordIntents.MessageContents;

    public static async Task Main()
    {
        IServiceProvider services = await ConfigureServices();

        var config = services.GetRequiredService<TicketBotConfig>();
        var bot = services.GetRequiredService<TicketBot>();

        IDisposable? sentry = null;
        //TODO: Rewrite this mess.

        if (config.Sentry != null)
        {
            sentry = SentrySdk.Init(o =>
            {
                o.Dsn = config.Sentry.Dsn;
                o.TracesSampleRate = config.Sentry.TracesSampleRate;
#if DEBUG
                o.Environment = "development";
#else
                o.Environment = "production";
#endif
            });
        }

        await bot.StartAsync();

        if (sentry != null)
        {
            using (sentry)
            {
                await Task.Delay(Timeout.Infinite);
            }
        }
        else
        {
            await Task.Delay(Timeout.Infinite);
        }
    }

    private static async Task<IServiceProvider> ConfigureServices()
    {
        IServiceCollection services = new ServiceCollection();

        var console = new ConsoleLog();
        var ver = typeof(Program).Assembly.GetName().Version;
        await console.LogInfo($"Hello World. {nameof(TicketBot)} v{ver}");

        var config = ConfigHelper.GetConfig();

        var errorLog = new WebhookLog(new DiscordWebhook(config.Discord.WebhookUrl));
        await errorLog.LogInfo("Startup webhook test.");

        var log = new ErrorLogHandler(console, errorLog);

        var discord = new DiscordClient(new DiscordConfiguration()
        {
            Token = config.Discord.Token,
            Intents = REQUIRED_INTENTS
        });

        if (config.Mongo != null)
        {
            await services.ConfigureMongoDb(config.Mongo, log);
        }

        //if (config.Web != null)
        //{
        //    services.ConfigureWebServices(config.Web);
        //}

        services
            .AddSingleton(config)
            .AddSingleton(discord)
            .AddSingleton<IAsyncLog>(log)
            .ConfigureAppServices()
            .AddSingleton(x => discord.UseInteractivity(new InteractivityConfiguration()))
            .AddSingleton((x) => discord.UseSlashCommands(new SlashCommandsConfiguration()
            {
                Services = x,
            }))
            .AddSingleton<TicketBot>();

        return services.BuildServiceProvider();
    }
}
