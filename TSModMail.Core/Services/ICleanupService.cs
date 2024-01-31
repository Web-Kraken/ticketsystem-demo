using System.Threading.Tasks;
using TSModMail.Core.Entities;

namespace TSModMail.Core.Services;

/// <summary>
/// Cleans up ticket attachments. 
/// </summary>
public interface ICleanupService
{
    /// <summary>
    /// Cleanup tickets that have been left open too long without opening.
    /// </summary>
    Task CleanupQuestioningTickets(TicketRegion guild);

    /// <summary>
    /// Cleanup attachments that are outside of the retention policy.
    /// </summary>
    Task CleanupOldAttachments(TicketRegion guild);

    /// <summary>
    /// Remove the receipts from ticket log attachments.
    /// </summary>
    Task CleanupOldTicketLogs(TicketRegion guild);
}
