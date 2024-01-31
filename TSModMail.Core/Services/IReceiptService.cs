using DSharpPlus.Entities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TSModMail.Core.Entities.Tickets;

namespace TSModMail.Core.Services;

/// <summary>
/// Service for generating receipts for closing tickets.
/// </summary>
public interface IReceiptService
{
    /// <summary>
    /// Generate a receipt for a ticket.
    /// </summary>
    Task<Stream> GenerateReceipt(
        TicketCloseRequest req,
        IEnumerable<DiscordMessage> messages,
        DiscordChannel channel
    );
}
