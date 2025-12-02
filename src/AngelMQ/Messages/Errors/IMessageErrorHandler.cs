using AngelMQ.Properties;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AngelMQ.Messages.Errors;

public interface IMessageErrorHandler
{
    Task HandleAsync<TMessage>(QueueProperties<TMessage> queueProperties,
                               IChannel messageChannel,
                               BasicDeliverEventArgs args,
                               Exception exception) where TMessage : class;
}
