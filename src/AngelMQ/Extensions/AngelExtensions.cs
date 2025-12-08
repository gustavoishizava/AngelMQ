using AngelMQ.Properties;
using Microsoft.Extensions.DependencyInjection;

namespace AngelMQ.Extensions;

public static class AngelExtensions
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, Action<ConnectionProperties> configure)
    {
        services.Configure<ConnectionProperties>(options => configure(options))
                .AddConnectionFactory()
                .AddMessagePublisher()
                .AddChannels()
                .AddConsumersManagement()
                .AddMessagePublisher();

        return services;
    }
}
