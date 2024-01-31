using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using TSModMail.Application.Services;
using TSModMail.Application.Services.Commands;
using TSModMail.Application.Services.Tickets;
using TSModMail.Core.Entities;
using TSModMail.Core.Services;
using TSModMail.Core.Services.Tickets;

namespace TSModMail.Application.Helpers;

public static class ServiceExtensions
{
    public static IServiceCollection ConfigureAppServices(this IServiceCollection @this)
    {
        return @this
        .AddSingleton<IRegionService, RegionService>()
        .AddSingleton<IPermissionService, PermissionService>()
        .AddSingleton<ITicketService, TicketService>()
        .AddSingleton<ICommandService, CommandService>()
        .AddSingleton<IMenuService, MenuService>()
        .AddSingleton<IReceiptService, ReceiptService>()
        .AddSingleton<IQuestionnaireService, QuestionnaireService>()
        .AddSingleton<ITicketHistoryService, TicketHistoryService>()
        .AddSingleton<ITicketService, TicketService>()
        .AddSingleton<ICleanupService, CleanupService>()
        //.AddSingleton<IAttachmentService, AttachmentService>() // This works but I have to limit it.
        .AddSingleton<IAttachmentService, DummyAttachmentService>() // TODO: REMOVE DUMMY IMPLEMENTATION
        .AddSingleton<IMessageService, MessageService>()
        .AddSingleton<INotificationService, DiscordNotificationService>()
        .AddSingleton<ITicketWatchdogService, TicketWatchdogService>();
    }

    //TODO: Why is this here?
    public static async Task<TicketRegion?> TryGetRegion(
        this IRegionService @this,
        string? regionName,
        InteractionContext ctx,
        BotPermissionLevel desiredPerms,
        IPermissionService perms
    )
    {
        if (regionName == null) // No name specified
        {
            var validRegions = await perms.GetValidRegions(ctx.Member, desiredPerms);
            if (validRegions.Count() > 1) // multiple valid
            {
                await ctx.EditResponsePlain("You have permissions on more than one region, please specify.");
                return null;
            }
            if (validRegions.Count() == 0) // no valid
            {
                await ctx.EditResponsePlain("You do not have permission to use this.");
                return null;
            }

            return validRegions.First();
        }

        var namedRegion = await @this.GetByName(ctx.Guild, regionName);
        if (namedRegion != null) // input is nickname
        {
            return await CheckPerm(ctx, desiredPerms, perms, namedRegion);
        }

        var regions = await @this.GetByGuild(ctx.Guild);
        var idRegion = regions.FirstOrDefault(x => x.Id.ToString().StartsWith(regionName));
        if (idRegion != null) // input is id
        {
            return await CheckPerm(ctx, desiredPerms, perms, idRegion);
        }

        await ctx.EditResponsePlain($"Region {regionName} not found.");
        return null;
    }

    private static async Task<TicketRegion?> CheckPerm(InteractionContext ctx, BotPermissionLevel desiredPerms, IPermissionService perms, TicketRegion namedRegion)
    {
        var namedPerm = await perms.GetForMember(ctx.Member, namedRegion.Id);
        if (namedPerm == null || namedPerm.Level < desiredPerms)
        {
            await ctx.EditResponsePlain("You do not have permission to do this.");
            return null;
        }

        return namedRegion;
    }
}
