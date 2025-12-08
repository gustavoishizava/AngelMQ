using AngelMQ.Consumers;
using AngelMQ.Consumers.Workers;
using AngelMQ.Consumers.Workers.Abstractions;
using AngelMQ.Messages;
using AngelMQ.Properties.Consumers;
using AngelMQ.Queues;
using Microsoft.Extensions.DependencyInjection;

namespace AngelMQ.Extensions;

public static class ConsumerExtensions
{
    internal static IServiceCollection AddConsumersManagement(this IServiceCollection services)
    {
        services.AddSingleton<IConsumerFactory, ConsumerFactory>()
                .AddSingleton<IQueueFactory, QueueFactory>();

        return services;
    }

    public static IServiceCollection AddConsumer<TMessageHandler, TMessage>(this IServiceCollection services,
        Action<QueueProperties<TMessage>> configureQueueProperties)
        where TMessageHandler : class, IMessageHandler<TMessage>
        where TMessage : class
    {
        services.AddTransient<IMessageHandler<TMessage>, TMessageHandler>()
            .Configure(configureQueueProperties);

        services.AddHostedService<SimpleQueueWorker<TMessage>>();

        return services;
    }

    public static IServiceCollection AddConsumer<TQueueProvider, TMessageHandler, TMessage>(
        this IServiceCollection services)
        where TQueueProvider : class, IQueueProvider<TMessage>
        where TMessageHandler : class, IMessageHandler<TMessage>
        where TMessage : class
    {
        services.AddTransient<IMessageHandler<TMessage>, TMessageHandler>()
                .AddSingleton<IQueueProvider<TMessage>, TQueueProvider>();

        services.AddHostedService<MultiQueueWorker<TMessage>>();

        return services;
    }
}
