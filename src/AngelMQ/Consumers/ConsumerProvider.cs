using AngelMQ.Channels;
using AngelMQ.Messages;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AngelMQ.Consumers;

public sealed class ConsumerProvider(ILogger<ConsumerProvider> logger,
                                     IChannelProvider channelProvider) : IConsumerProvider
{
    public async Task<AsyncDefaultBasicConsumer> CreateConsumerAsync<TMessage>(
        IMessageHandler<TMessage> messageHandler) where TMessage : class
    {
        var channel = await channelProvider.GetChannelAsync();
        return BuildConsumer(channel, messageHandler);
    }

    private AsyncDefaultBasicConsumer BuildConsumer<TMessage>(IChannel channel,
                                                              IMessageHandler<TMessage> messageHandler)
        where TMessage : class
    {
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.RegisteredAsync += OnRegisteredAsync;
        consumer.ReceivedAsync += (sender, args) => OnReceivedAsync(sender, args, messageHandler);
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
                                                 IMessageHandler<TMessage> messageHandler) where TMessage : class
    {
        var consumer = (AsyncEventingBasicConsumer)sender!;

        logger.LogInformation("Message received: {DeliveryTag}",
                              args.DeliveryTag);

        try
        {
            await messageHandler.HandleAsync(args);
            await consumer.Channel.BasicAckAsync(args.DeliveryTag, multiple: false);
            logger.LogInformation("ACK sent successfully for {DeliveryTag}", args.DeliveryTag);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending ACK for {DeliveryTag}", args.DeliveryTag);
        }
    }

    private Task OnShutdownAsync(object? sender, ShutdownEventArgs args)
    {
        logger.LogWarning("Consumer shutdown: {ReplyText}",
                          args.ReplyText);
        return Task.CompletedTask;
    }
}
