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
            return properties.ConnectionFactory;
        });

        services.AddKeyedSingleton<IConnectionProvider, ConnectionProvider>(ConnectionNames.Consumer)
                .AddKeyedSingleton<IConnectionProvider, ConnectionProvider>(ConnectionNames.Publisher);

        return services;
    }
}
