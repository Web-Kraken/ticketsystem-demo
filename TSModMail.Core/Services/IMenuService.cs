using DSharpPlus;
using DSharpPlus.EventArgs;
using System.Threading.Tasks;

namespace TSModMail.Core.Services;

/// <summary>
/// Service for handing Discord interactive components.
/// </summary>
public interface IMenuService
{
    /// <summary>
    /// Handle a Discord button event and pass it to respective tickets.
    /// </summary>
    Task HandleComponentInteract(
        DiscordClient sender,
        ComponentInteractionCreateEventArgs e
    );

    /// <summary>
    /// Handle callback of a modal being submitted.
    /// </summary>
    Task HandleModalSubmitted(DiscordClient sender, ModalSubmitEventArgs e);
}
