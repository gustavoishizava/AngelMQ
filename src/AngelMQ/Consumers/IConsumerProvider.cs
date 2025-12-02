using AngelMQ.Messages;
using RabbitMQ.Client;

namespace AngelMQ.Consumers;

public interface IConsumerProvider
{
    Task<AsyncDefaultBasicConsumer> CreateConsumerAsync<TMessage>(
        IMessageHandler<TMessage> messageHandler, ushort prefetchCount = 1) where TMessage : class;
}
