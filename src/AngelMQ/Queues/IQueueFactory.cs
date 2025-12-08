using AngelMQ.Properties.Consumers;
using RabbitMQ.Client;

namespace AngelMQ.Queues;

public interface IQueueFactory
{
    Task CreateAsync<TMessage>(IChannel channel,
                               QueueProperties<TMessage> queueProperties) where TMessage : class;
}
