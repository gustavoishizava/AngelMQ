using AngelMQ.Messages;
using AngelMQ.Messages.Errors;
using AngelMQ.Properties;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AngelMQ.Consumers;

public sealed class ConsumerProvider(ILogger<ConsumerProvider> logger,
                                     IMessageErrorHandler messageErrorHandler) : IConsumerProvider
{
    public async Task<AsyncDefaultBasicConsumer> CreateConsumerAsync<TMessage>(
        IChannel channel,
        IMessageHandler<TMessage> messageHandler,
        QueueProperties<TMessage> queueProperties) where TMessage : class
    {
        return BuildConsumer(channel, messageHandler, queueProperties);
    }

    private AsyncDefaultBasicConsumer BuildConsumer<TMessage>(IChannel channel,
                                                              IMessageHandler<TMessage> messageHandler,
                                                              QueueProperties<TMessage> queueProperties)
        where TMessage : class
    {
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.RegisteredAsync += OnRegisteredAsync;
        consumer.ReceivedAsync += (sender, args) => OnReceivedAsync(sender,
                                                                    args,
                                                                    messageHandler,
                                                                    queueProperties);
        consumer.ShutdownAsync += OnShutdownAsync;

        return consumer;
    }

    private Task OnRegisteredAsync(object? sender, ConsumerEventArgs args)
    {
        logger.LogInformation("Consumer registered: {ConsumerTag}",
                              string.Join(",", args.ConsumerTags));
        return Task.CompletedTask;
    }

    private async Task OnReceivedAsync<TMessage>(object? sender,
                                                 BasicDeliverEventArgs args,
                                                 IMessageHandler<TMessage> messageHandler,
                                                 QueueProperties<TMessage> queueProperties) where TMessage : class
    {
        var consumer = (AsyncEventingBasicConsumer)sender!;

        logger.LogDebug("Message received: {DeliveryTag}", args.DeliveryTag);

        try
        {
            await messageHandler.HandleAsync(args);
            await consumer.Channel.BasicAckAsync(args.DeliveryTag, multiple: false);

            logger.LogDebug("ACK sent successfully for {DeliveryTag}", args.DeliveryTag);
        }
        catch (Exception ex)
        {
            await messageErrorHandler.HandleAsync(queueProperties,
                                                  consumer.Channel,
                                                  args,
                                                  ex);
        }
    }

    private Task OnShutdownAsync(object? sender, ShutdownEventArgs args)
    {
        logger.LogWarning("Consumer shutdown: {ReplyText}",
                          args.ReplyText);
        return Task.CompletedTask;
    }
}
