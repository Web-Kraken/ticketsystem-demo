using System.Threading.Tasks;
using TSModMail.Core.Entities.Tickets;

namespace TSModMail.Core.Services;

/// <summary>
/// Service for opening and handling questionnaires.
/// </summary>
public interface IQuestionnaireService
{
    Task HandleTicketOpen(TicketOpenRequest tor);
}
