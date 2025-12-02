using System.Collections.Concurrent;
using AngelMQ.Connections;
using AngelMQ.Properties;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AngelMQ.Channels.Pool;

public sealed class ChannelPool(ILogger<ChannelPool> logger,
                                IConnectionProvider connectionProvider,
                                IOptions<ChannelPoolProperties> poolPropertiesOptions) : IChannelPool, IAsyncDisposable, IDisposable
{
    private readonly SemaphoreSlim _semaphore =
        new(poolPropertiesOptions.Value.MaxPoolSize, poolPropertiesOptions.Value.MaxPoolSize);
    private readonly ConcurrentStack<IChannel> _channels = new();

    private int _currentPoolSize = 0;

    public async Task<IChannel> GetAsync()
    {
        await _semaphore.WaitAsync();

        if (_channels.TryPop(out var channel))
        {
            if (channel.IsOpen)
                return channel;

            logger.LogInformation("Disposing closed RabbitMQ channel. Current pool size: {CurrentPoolSize}", _currentPoolSize);
            Interlocked.Decrement(ref _currentPoolSize);
            await channel.DisposeAsync();
        }

        if (Interlocked.Increment(ref _currentPoolSize) <= poolPropertiesOptions.Value.MaxPoolSize)
        {
            logger.LogInformation("Creating new RabbitMQ channel. Current pool size: {CurrentPoolSize}", _currentPoolSize);
            var newChannel = await CreateChannelAsync();
            return newChannel;
        }

        throw new InvalidOperationException("RabbitMQ channel pool is exhausted.");
    }

    public async Task ReturnAsync(IChannel channel)
    {
        try
        {
            if (!channel.IsOpen)
            {
                await channel.DisposeAsync();
                Interlocked.Decrement(ref _currentPoolSize);
                return;
            }

            _channels.Push(channel);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<IChannel> CreateChannelAsync()
    {
        var connection = await connectionProvider.GetConnectionAsync(ConnectionType.Publisher);
        return await connection.CreateChannelAsync();
    }

    public void Dispose() =>
        DisposeAsync().AsTask().GetAwaiter().GetResult();

    public async ValueTask DisposeAsync()
    {
        while (_channels.TryPop(out var channel))
            await channel.DisposeAsync();

        _semaphore.Dispose();
    }
}
