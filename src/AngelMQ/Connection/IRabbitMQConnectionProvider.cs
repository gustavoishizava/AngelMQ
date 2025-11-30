using RabbitMQ.Client;

namespace AngelMQ.Connection;

public interface IRabbitMQConnectionProvider
{
    Task<IConnection> GetConnectionAsync();
}
