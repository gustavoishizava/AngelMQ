using AngelMQ.Messages;
using AngelMQ.Properties;
using Microsoft.Extensions.DependencyInjection;

namespace AngelMQ.Extensions;

public static class ConsumerExtensions
{
    public static IServiceCollection AddConsumer<TMessageHandler, TMessage>(this IServiceCollection services,
        Action<QueueProperties<TMessage>> configureQueueProperties)
        where TMessageHandler : class, IMessageHandler<TMessage>
        where TMessage : class
    {

        services.AddTransient<IMessageHandler<TMessage>, TMessageHandler>()
            .Configure(configureQueueProperties);

        return services;
    }
}
