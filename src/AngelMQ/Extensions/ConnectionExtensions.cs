using AngelMQ.Channels;
using AngelMQ.Connections;
using AngelMQ.Properties;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace AngelMQ.Extensions;

public static class ConnectionExtensions
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, Action<ConnectionProperties> configure)
    {
        ConnectionProperties properties = new();
        configure(properties);

        services.AddConnectionFactory(properties)
                .AddTransient<IRabbitMQConnectionProvider, RabbitMQConnectionProvider>()
                .AddTransient<IChannelProvider, ChannelProvider>();

        return services;
    }

    private static IServiceCollection AddConnectionFactory(this IServiceCollection services, ConnectionProperties properties)
    {
        services.AddSingleton<IConnectionFactory>(_ =>
        {
            return new ConnectionFactory
            {
                HostName = properties.HostName,
                Port = properties.Port,
                UserName = properties.UserName,
                Password = properties.Password,
                VirtualHost = properties.VirtualHost,
                AutomaticRecoveryEnabled = true,
                ConsumerDispatchConcurrency = properties.ConsumerDispatchConcurrency
            };
        });

        return services;
    }
}
