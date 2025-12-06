using AngelMQ.Messages.Errors;
using AngelMQ.Properties.Publishers;
using AngelMQ.Publishers;
using AngelMQ.Publishers.Base;
using Microsoft.Extensions.DependencyInjection;

namespace AngelMQ.Extensions;

public static class PublisherExtensions
{
    internal static IServiceCollection AddMessagePublisher(this IServiceCollection services)
    {
        return services.AddSingleton<IMessagePublisher, MessagePublisher>()
                       .AddSingleton<IMessageErrorHandler, MessageErrorHandler>();
    }

    public static IServiceCollection AddExchangePublisher<TMessage, TExchangePublisher>(
        this IServiceCollection services,
        Action<PublisherProperties<TMessage>> configure)
        where TMessage : class
        where TExchangePublisher : ExchangePublisher<TMessage>
    {
        services.AddSingleton<IPublisher<TMessage>, TExchangePublisher>()
                .Configure(configure);

        return services;
    }

    public static IServiceCollection AddQueuePublisher<TMessage, TQueuePublisher>(
        this IServiceCollection services,
        Action<PublisherProperties<TMessage>> configure)
        where TMessage : class
        where TQueuePublisher : QueuePublisher<TMessage>
    {
        services.AddSingleton<IPublisher<TMessage>, TQueuePublisher>()
                .Configure(configure);

        return services;
    }
}
