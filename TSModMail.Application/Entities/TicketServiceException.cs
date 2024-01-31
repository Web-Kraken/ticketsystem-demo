using System;

namespace TSModMail.Application.Entities;

public sealed class TicketServiceException : Exception
{
    public TicketServiceException()
    {
    }

    public TicketServiceException(string? message) : base(message)
    {
    }

    public TicketServiceException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
