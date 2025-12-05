using RabbitMQ.Client;

namespace AngelMQ.Channels.Pool;

public interface IChannelPool
{
    Task<IChannel> GetAsync();
    Task ReturnAsync(IChannel channel);
    int CurrentPoolSize { get; }
}
