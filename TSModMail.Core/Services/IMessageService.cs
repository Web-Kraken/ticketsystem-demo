using DSharpPlus;
using DSharpPlus.EventArgs;
using System.Threading.Tasks;

namespace TSModMail.Core.Services;

/// <summary>
/// Service for handling Discord messages.
/// </summary>
public interface IMessageService
{
    /// <summary>
    /// Handles a Discord message event by redistributing it to associated 
    /// services.
    /// </summary>
    Task HandleMessage(DiscordClient client, MessageCreateEventArgs msg);
}
