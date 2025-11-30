using RabbitMQ.Client;

namespace AngelMQ.Connections;

public interface IRabbitMQConnectionProvider
{
    Task<IConnection> GetConnectionAsync();
}
