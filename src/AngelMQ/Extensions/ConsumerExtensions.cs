using AngelMQ.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace AngelMQ.Extensions;

public static class ConsumerExtensions
{
    public static IServiceCollection AddConsumer<TMessageHandler, TMessage>(this IServiceCollection services)
        where TMessageHandler : class, IMessageHandler<TMessage>
        where TMessage : class
    {
        services.AddTransient<IMessageHandler<TMessage>, TMessageHandler>();

        return services;
    }
}
