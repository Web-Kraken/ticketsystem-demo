using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using SLog;
using System;
using System.Threading.Tasks;
using TSModMail.Core.Entities;
using TSModMail.Core.Entities.Menus;
using TSModMail.Core.Entities.Tickets;
using TSModMail.Core.Repositories;
using TSModMail.Repositories.MongoDb.Entities;

namespace TSModMail.Repositories.MongoDb.Helpers;

public static class ServiceExtensions
{
    private const string MONGO_PERM_COL = "permissions";
    private const string MONGO_TICKET_COL = "tickets";
    private const string MONGO_CONFIG_COL = "config";
    private const string MONGO_MENU_COL = "menus";
    private const string MONGO_ATTACHMENT_COL = "attachments";
    private const string MONGO_HISTORY_COL = "history";
    private const string MONGO_MODAL_COL = "modals";

    public static async Task ConfigureMongoDb(
        this IServiceCollection @this,
        MongoConfig config,
        IAsyncLog log
    )
    {
        var mongo = new MongoClient(config.ConnectionString);

        await log.LogInfo("Connecting to MongoDB...");

        try
        {
            await mongo.StartSessionAsync();
        }
        catch (Exception e)
        {
            await log.LogError($"Have you considered opening Mongo.", e);
            throw;
        }

        var db = mongo.GetDatabase(config.DatabaseName);

        @this
            .AddSingleton<IPermissionRepository>(
                new MongoPermissionRepository(
                    db.GetCollection<PermissionRecord>(MONGO_PERM_COL)
                )
            )
            .AddSingleton<IMenuRepository>(
                new MongoMenuRepository(
                    db.GetCollection<GuildMenus>(MONGO_MENU_COL)
                )
            )
            .AddSingleton<ITicketHistoryRepository>(
                new MongoTicketHistoryRepository(
                    db.GetCollection<HistoricTicket>(MONGO_HISTORY_COL)
                )
            )
            .AddSingleton<ITicketRepository>(
                new MongoTicketRepository(
                    db.GetCollection<Ticket>(MONGO_TICKET_COL)
                )
            )
            .AddSingleton<ITicketRegionRepository>(
                new MongoRegionRepository(
                    db.GetCollection<TicketRegion>(MONGO_CONFIG_COL)
                )
            )
            .AddSingleton<IAttachmentRepository>(
                new MongoAttachmentRepository(
                    db.GetCollection<AttachmentMap>(MONGO_ATTACHMENT_COL)
                )
            )
            .AddSingleton<IModalRepository>(
                new MongoModalRepository(
                    db.GetCollection<MenuModal>(MONGO_MODAL_COL)
                )
            );
    }
}
