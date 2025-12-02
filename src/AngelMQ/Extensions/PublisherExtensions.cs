using AngelMQ.Channels.Pool;
using AngelMQ.Properties;
using AngelMQ.Publishers;
using Microsoft.Extensions.DependencyInjection;

namespace AngelMQ.Extensions;

public static class PublisherExtensions
{
    public static IServiceCollection AddMessagePublisher(this IServiceCollection services, int? maxPoolSize = null)
    {
        return services.AddSingleton<IMessagePublisher, MessagePublisher>()
            .AddSingleton<IChannelPool, ChannelPool>()
            .Configure<ChannelPoolProperties>(options =>
            {
                if (maxPoolSize.HasValue)
                    options.SetMaxPoolSize(maxPoolSize.Value);
            });
    }
}
