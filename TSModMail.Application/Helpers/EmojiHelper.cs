using System;
using TSModMail.Core.Entities.Tickets;

namespace TSModMail.Application.Helpers;

internal static class EmojiHelper
{
    public static string GetTSEmoji(TicketWaitStatus wait)
    {
        switch (wait)
        {
            case TicketWaitStatus.WaitingOnTS: return "🟠"; // Orange Circle
            case TicketWaitStatus.WaitingOnTSLate: return "🔴"; // Red Circle
            case TicketWaitStatus.WaitingOnUser: return "🟢"; // Green Circle
            case TicketWaitStatus.WaitingOnUserLate: return "⭕"; // Hollow Red Circle
            default: throw new ArgumentException("Please use a valid status.");
        }
    }
}
