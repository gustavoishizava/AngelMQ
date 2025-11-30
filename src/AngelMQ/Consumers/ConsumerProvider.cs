using AngelMQ.Channels;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AngelMQ.Consumers;

public sealed class ConsumerProvider(ILogger<ConsumerProvider> logger,
                                     IChannelProvider channelProvider) : IConsumerProvider
{
    public async Task<AsyncDefaultBasicConsumer> CreateConsumerAsync()
    {
        var channel = await channelProvider.GetChannelAsync();
        return BuildConsumer(channel);
    }

    private AsyncDefaultBasicConsumer BuildConsumer(IChannel channel)
    {
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.RegisteredAsync += OnRegisteredAsync;
        consumer.ReceivedAsync += OnReceivedAsync;
        consumer.ShutdownAsync += OnShutdownAsync;

        return consumer;
    }

    private Task OnRegisteredAsync(object? sender, ConsumerEventArgs args)
    {
        logger.LogInformation("Consumer registered: {ConsumerTag}",
                              string.Join(",", args.ConsumerTags));
        return Task.CompletedTask;
    }

    private async Task OnReceivedAsync(object? sender, BasicDeliverEventArgs args)
    {
        var consumer = (AsyncEventingBasicConsumer)sender!;

        logger.LogInformation("Message received: {DeliveryTag}",
                              args.DeliveryTag);

        try
        {
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
