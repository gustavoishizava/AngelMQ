using AngelMQ.Consumers;
using AngelMQ.Consumers.Workers;
using AngelMQ.Messages;
using AngelMQ.Properties;
using AngelMQ.Queues;
using Microsoft.Extensions.DependencyInjection;

namespace AngelMQ.Extensions;

public static class ConsumerExtensions
{
    internal static IServiceCollection AddConsumersManagement(this IServiceCollection services)
    {
        services.AddSingleton<IConsumerFactory, ConsumerFactory>()
                .AddSingleton<IQueueSetup, QueueSetup>();

        return services;
    }

    public static IServiceCollection AddConsumer<TMessageHandler, TMessage>(this IServiceCollection services,
        Action<QueueProperties<TMessage>> configureQueueProperties)
        where TMessageHandler : class, IMessageHandler<TMessage>
        where TMessage : class
    {
        services.AddTransient<IMessageHandler<TMessage>, TMessageHandler>()
            .Configure(configureQueueProperties);

        services.AddHostedService<QueueWorker<TMessage>>();

        return services;
    }
}
