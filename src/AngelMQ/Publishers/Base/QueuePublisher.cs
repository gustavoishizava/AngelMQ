using AngelMQ.Channels.Pool;
using AngelMQ.Properties.Publishers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AngelMQ.Publishers.Base;

public abstract class QueuePublisher<TMessage>
    : PublisherBase<TMessage, QueueProperties> where TMessage : class
{
    public QueuePublisher(ILogger<PublisherBase<TMessage, QueueProperties>> logger,
                          IChannelPool channelPool,
                          IMessagePublisher messagePublisher,
                          IOptions<PublisherProperties<TMessage, QueueProperties>> options)
                          : base(logger, channelPool, messagePublisher, options)
    {
    }

    protected override async Task SetupAsync(IChannel channel)
    {
        if (!properties.IsQueuePublisher)
            throw new InvalidOperationException("PublisherProperties does not contain Queue properties.");

        var queue = properties.Configuration;
        queue.Validate();

        logger.LogInformation("Creating queue {QueueName}.", queue.Name);

        await channel.QueueDeclareAsync(queue: queue.Name,
                                        durable: queue.Durable,
                                        exclusive: queue.Exclusive,
                                        autoDelete: queue.AutoDelete,
                                        arguments: queue.Arguments,
                                        passive: queue.Passive,
                                        noWait: queue.NoWait);
    }

    protected override Task SendAsync(TMessage message, IDictionary<string, string>? headers = null)
    {
        var queueName = properties.Configuration.Name;

        return messagePublisher.PublishAsync(message, string.Empty, queueName, headers);
    }
}