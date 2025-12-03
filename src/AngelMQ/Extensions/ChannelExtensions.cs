using AngelMQ.Channels;
using AngelMQ.Channels.Pool;
using Microsoft.Extensions.DependencyInjection;

namespace AngelMQ.Extensions;

internal static class ChannelExtensions
{
    internal static IServiceCollection AddChannels(this IServiceCollection services)
    {
        services.AddScoped<IChannelProvider, ChannelProvider>()
                .AddSingleton<IChannelPool, ChannelPool>();

        return services;
    }
}
