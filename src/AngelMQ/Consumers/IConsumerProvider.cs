using AngelMQ.Messages;
using AngelMQ.Properties;
using RabbitMQ.Client;

namespace AngelMQ.Consumers;

public interface IConsumerProvider
{
    Task<AsyncDefaultBasicConsumer> CreateConsumerAsync<TMessage>(
        IChannel channel,
        IMessageHandler<TMessage> messageHandler,
        QueueProperties<TMessage> queueProperties)
        where TMessage : class;
}
