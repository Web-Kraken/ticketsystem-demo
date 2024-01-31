using System;
using System.Threading.Tasks;
using TSModMail.Core.Entities.Tickets;
using TSModMail.Core.Services;

namespace TSModMail.Application.Services;

/// <inheritdoc cref="IQuestionnaireService"/>
public class QuestionnaireService : IQuestionnaireService
{
    public Task HandleTicketOpen(TicketOpenRequest tor)
    {
        throw new NotImplementedException();
    }
}
