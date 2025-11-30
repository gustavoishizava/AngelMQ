using RabbitMQ.Client;

namespace AngelMQ.Channels;

public interface IChannelProvider
{
    Task<IChannel> GetChannelAsync(ushort prefetchCount = 1);
    Task CloseChannelAsync();
}
