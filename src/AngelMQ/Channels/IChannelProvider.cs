using RabbitMQ.Client;

namespace AngelMQ.Channels;

public interface IChannelProvider
{
    Task<IChannel> GetAsync(ushort prefetchCount = 1);
    Task CloseAsync();
}
