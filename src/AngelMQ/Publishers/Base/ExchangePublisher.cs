using AngelMQ.Channels.Pool;
using AngelMQ.Properties.Publishers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AngelMQ.Publishers.Base;

public abstract class ExchangePublisher<TMessage>
    : PublisherBase<TMessage, ExchangeProperties> where TMessage : class
{
    protected ExchangePublisher(ILogger<PublisherBase<TMessage, ExchangeProperties>> logger,
                                IChannelPool channelPool,
                                IMessagePublisher messagePublisher,
                                IOptions<PublisherProperties<TMessage, ExchangeProperties>> options)
        : base(logger, channelPool, messagePublisher, options)
    {
    }

    protected override async Task SetupAsync(IChannel channel)
    {
        if (!properties.IsExchangePublisher)
            throw new InvalidOperationException("PublisherProperties does not contain Exchange properties.");

        var exchange = properties.Configuration;
        exchange.Validate();

        logger.LogInformation("Creating exchange {ExchangeName}.", exchange);

        await channel.ExchangeDeclareAsync(exchange: exchange.Name,
                                           type: exchange.Type,
                                           durable: exchange.Durable,
                                           autoDelete: exchange.AutoDelete,
                                           arguments: exchange.Arguments,
                                           passive: exchange.Passive,
                                           noWait: exchange.NoWait);
    }

    protected override Task SendAsync(TMessage message,
                                      IDictionary<string, string>? headers = null)
    {
        var exchangeName = properties.Configuration.Name;
        var routingKey = BuildRoutingKey(message, headers);

        return messagePublisher.PublishAsync(message, exchangeName, routingKey, headers);
    }

    protected abstract string BuildRoutingKey(TMessage message, IDictionary<string, string>? headers = null);
}