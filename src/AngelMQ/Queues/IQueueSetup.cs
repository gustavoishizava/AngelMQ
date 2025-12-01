using AngelMQ.Properties;
using RabbitMQ.Client;

namespace AngelMQ.Queues;

public interface IQueueSetup
{
    Task CreateQueueAsync<TMessage>(IChannel channel,
                          QueueProperties<TMessage> queueProperties) where TMessage : class;
}
