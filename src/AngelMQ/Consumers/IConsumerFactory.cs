using AngelMQ.Messages;
using AngelMQ.Properties.Consumers;
using RabbitMQ.Client;

namespace AngelMQ.Consumers;

public interface IConsumerFactory
{
    AsyncDefaultBasicConsumer Create<TMessage>(
        IChannel channel,
        IMessageHandler<TMessage> messageHandler,
        QueueProperties<TMessage> queueProperties)
        where TMessage : class;
}
