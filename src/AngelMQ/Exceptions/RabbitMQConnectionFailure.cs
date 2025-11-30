using System;

namespace AngelMQ.Exceptions;

public sealed class RabbitMQConnectionFailure : Exception
{
    public RabbitMQConnectionFailure(string message)
        : base(message)
    {
    }
}
