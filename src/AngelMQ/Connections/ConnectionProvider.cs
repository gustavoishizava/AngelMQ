using AngelMQ.Exceptions;
using AngelMQ.Properties;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace AngelMQ.Connections;

public sealed class ConnectionProvider(ILogger<ConnectionProvider> logger,
                                       IConnectionFactory connectionFactory,
                                       IOptions<ConnectionProperties> options) : IConnectionProvider
{
    private Dictionary<ConnectionType, IConnection> _connections = new();
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    public async Task<IConnection> GetConnectionAsync(ConnectionType connectionType)
    {
        if (IsConnected(connectionType))
            return _connections[connectionType]!;

        await _connectionLock.WaitAsync();

        if (IsConnected(connectionType))
        {
            _connectionLock.Release();
            return _connections[connectionType]!;
        }

        try
        {
            var retryPolicy = BuildRetryPolicy();

            IConnection? connection = null;

            await retryPolicy.Execute(async () =>
            {
                logger.LogInformation("Attempting to connect to RabbitMQ...");

                connection = await connectionFactory.CreateConnectionAsync();
                connection.ConnectionShutdownAsync += OnConnectionShutdownAsync;

                _connections[connectionType] = connection;

                logger.LogInformation("Successfully connected to RabbitMQ.");
            });

            if (connection is null)
                throw new RabbitMQConnectionFailure("Failed to establish a connection to RabbitMQ.");

            return connection!;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private bool IsConnected(ConnectionType connectionType)
    {
        return _connections.ContainsKey(connectionType) && (_connections[connectionType]?.IsOpen ?? false);
    }

    private RetryPolicy BuildRetryPolicy()
    {
        return Policy.Handle<ConnectFailureException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(options.Value.MaxRetryAttempts, retryAttemp =>
                {
                    logger.LogInformation($"Attemp #{retryAttemp}: connecting to RabbitMQ.");
                    return TimeSpan.FromSeconds(Math.Pow(options.Value.DelayMultiplier, retryAttemp));
                });
    }

    private Task OnConnectionShutdownAsync(object _, ShutdownEventArgs args)
    {
        logger.LogWarning("RabbitMQ connection has been shut down.");
        return Task.CompletedTask;
    }
}
