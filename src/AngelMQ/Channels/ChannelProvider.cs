using AngelMQ.Connections;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace AngelMQ.Channels;

public sealed class ChannelProvider(ILogger<ChannelProvider> logger,
                                    IConnectionProvider connectionProvider) : IChannelProvider
{
    private IChannel? _channel;

    public async Task CloseChannelAsync()
    {
        if (_channel is null)
            return;

        await _channel.CloseAsync();
    }

    public async Task<IChannel> GetChannelAsync(ushort prefetchCount = 1)
    {
        if (_channel is not null)
            return _channel;

        logger.LogInformation("Creating new channel with prefetchCount: {PrefetchCount}",
                              prefetchCount);

        var connection = await connectionProvider.GetConnectionAsync();

        _channel = await connection.CreateChannelAsync();
        await _channel.BasicQosAsync(0,
                                     prefetchCount,
                                     global: false);

        return _channel;
    }
}