using AngelMQ.Properties;
using RabbitMQ.Client;

namespace AngelMQ.Consumers;

public interface IConsumerProvider
{
    Task<AsyncDefaultBasicConsumer> CreateConsumerAsync();
}
