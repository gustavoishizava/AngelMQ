using RabbitMQ.Client.Events;

namespace AngelMQ.Messages;

public interface IMessageHandler<in TMessage> where TMessage : class
{
    Task HandleAsync(BasicDeliverEventArgs args);
}
