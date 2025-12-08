using AngelMQ.Properties.Consumers;

namespace AngelMQ.Consumers.Workers.Abstractions;

public interface IQueueProvider<TMessage> where TMessage : class
{
    Task<List<QueueProperties<TMessage>>> GetQueuePropertiesAsync();
}
