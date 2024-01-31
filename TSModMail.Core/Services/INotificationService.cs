using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace TSModMail.Core.Services;

/// <summary>
/// A service that provides notifications of ticket changes.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Place a notification into the log channel.
    /// </summary>
    Task SendNotification(Guid regionId, DiscordMessageBuilder msg);
}
