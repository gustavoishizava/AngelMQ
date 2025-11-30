using AngelMQ.Exceptions;
using AngelMQ.Properties;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace AngelMQ.Connection;

public sealed class RabbitMQConnectionProvider(ILogger<RabbitMQConnectionProvider> logger,
                                               IConnectionFactory connectionFactory,
                                               IOptions<ConnectionProperties> options) : IRabbitMQConnectionProvider
{
    private IConnection? _connection;

    private bool _isConnected => _connection?.IsOpen ?? false;

    public async Task<IConnection> GetConnectionAsync()
    {
        if (_connection is not null && _isConnected)
            return _connection;

        var retryPolicy = BuildRetryPolicy();

        await retryPolicy.Execute(async () =>
        {
            logger.LogInformation("Attempting to connect to RabbitMQ...");
            _connection = await connectionFactory.CreateConnectionAsync();
            _connection.ConnectionShutdownAsync += OnConnectionShutdownAsync;
            logger.LogInformation("Successfully connected to RabbitMQ.");
        });

        if (_connection is null)
            throw new RabbitMQConnectionFailure("Failed to establish a connection to RabbitMQ.");

        return _connection;
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
