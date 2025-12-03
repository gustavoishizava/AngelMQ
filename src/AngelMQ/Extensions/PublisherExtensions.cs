using AngelMQ.Messages.Errors;
using AngelMQ.Publishers;
using Microsoft.Extensions.DependencyInjection;

namespace AngelMQ.Extensions;

internal static class PublisherExtensions
{
    internal static IServiceCollection AddMessagePublisher(this IServiceCollection services)
    {
        return services.AddSingleton<IMessagePublisher, MessagePublisher>()
                       .AddSingleton<IMessageErrorHandler, MessageErrorHandler>();
    }
}
