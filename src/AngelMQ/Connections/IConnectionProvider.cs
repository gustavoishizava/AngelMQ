using RabbitMQ.Client;

namespace AngelMQ.Connections;

public interface IConnectionProvider
{
    Task<IConnection> GetConnectionAsync(ConnectionType connectionType);
}
