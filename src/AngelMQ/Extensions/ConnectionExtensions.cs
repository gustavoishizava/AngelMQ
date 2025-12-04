using System.Security.Authentication;
using AngelMQ.Connections;
using AngelMQ.Constants;
using AngelMQ.Properties;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AngelMQ.Extensions;

internal static class ConnectionExtensions
{
    internal static IServiceCollection AddConnectionFactory(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionFactory>(serviceProvider =>
        {
            var properties = serviceProvider.GetRequiredService<IOptions<ConnectionProperties>>().Value;

            var connectionFactory = new ConnectionFactory
            {
                HostName = properties.HostName,
                Port = properties.Port,
                UserName = properties.UserName,
                Password = properties.Password,
                VirtualHost = properties.VirtualHost,
                AutomaticRecoveryEnabled = true,
                ConsumerDispatchConcurrency = properties.ConsumerDispatchConcurrency
            };

            if (properties.EnableSsl || properties.SslProtocols.HasValue)
            {
                connectionFactory.Ssl = new SslOption
                {
                    Enabled = properties.EnableSsl,
                    Version = properties.SslProtocols ?? SslProtocols.None
                };
            }

            return connectionFactory;
        });

        services.AddKeyedSingleton<IConnectionProvider, ConnectionProvider>(ConnectionNames.Consumer)
                .AddKeyedSingleton<IConnectionProvider, ConnectionProvider>(ConnectionNames.Publisher);

        return services;
    }
}
