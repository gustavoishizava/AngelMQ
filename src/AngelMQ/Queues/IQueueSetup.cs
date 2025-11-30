using AngelMQ.Properties;
using RabbitMQ.Client;

namespace AngelMQ.Queues;

public interface IQueueSetup
{
    Task CreateQueueAsync(IChannel channel,
                          QueueProperties queueProperties);
}
