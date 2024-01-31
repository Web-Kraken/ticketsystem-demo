using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;

namespace TSModMail.Application.Helpers;

/// <summary>
/// Implement some functionality that should come out of the box.
/// </summary>
internal static class InteractionExtensions
{
    public static Task RespondEphemeral(this InteractionContext @this, string content) => @this.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .AsEphemeral(true)
                .WithContent(content));

    public static Task RespondEphemeral(
        this DiscordInteraction @this,
        string content
        )
        => @this.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .AsEphemeral(true)
                .WithContent(content));

    public static Task EditResponsePlain(
        this BaseContext @this,
        string content
        )
        => @this.EditResponseAsync(
            new DiscordWebhookBuilder()
                .WithContent(content)
            );

}
