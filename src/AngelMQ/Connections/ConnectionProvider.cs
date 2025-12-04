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
    private IConnection? _connection;
    private bool _isConnected => _connection is not null && _connection.IsOpen;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    public async Task<IConnection> GetConnectionAsync()
    {
        if (_isConnected)
            return _connection!;

        await _connectionLock.WaitAsync();

        if (_isConnected)
        {
            _connectionLock.Release();
            return _connection!;
        }

        try
        {
            await BuildResiliencePipeline().ExecuteAsync(async _ =>
            {
                logger.LogInformation("Attempting to connect to RabbitMQ...");

                _connection = await connectionFactory.CreateConnectionAsync();
                _connection.ConnectionShutdownAsync += OnConnectionShutdownAsync;

                logger.LogInformation("Successfully connected to RabbitMQ.");
            });

            if (_connection is null)
                throw new RabbitMQConnectionFailure("Failed to establish a connection to RabbitMQ.");

            return _connection!;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private ResiliencePipeline BuildResiliencePipeline()
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = options.Value.MaxRetryAttempts,
                Delay = TimeSpan.FromSeconds(options.Value.DelayMultiplier),
                BackoffType = DelayBackoffType.Exponential,
                OnRetry = args =>
                {
                    logger.LogWarning("Connection attempt {RetryCount} failed. Retrying in {Delay} seconds...",
                                      args.AttemptNumber + 1, args.RetryDelay.TotalSeconds);

                    return default;
                },
                ShouldHandle = new PredicateBuilder()
                    .Handle<ConnectFailureException>()
                    .Handle<BrokerUnreachableException>()
            })
            .Build();
    }

    private Task OnConnectionShutdownAsync(object _, ShutdownEventArgs args)
    {
        logger.LogWarning("RabbitMQ connection has been shut down.");
        return Task.CompletedTask;
    }
}
