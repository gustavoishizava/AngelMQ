using AngelMQ.Channels;
using AngelMQ.Consumer.Listeners.Messages;
using AngelMQ.Consumers;
using AngelMQ.Messages;
using AngelMQ.Properties;
using AngelMQ.Queues;
using Microsoft.Extensions.Options;

namespace AngelMQ.Consumer.Listeners;

public sealed class ConsumerTest(IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var consumerProvider = scope.ServiceProvider.GetRequiredService<IConsumerProvider>();
        var channelProvider = scope.ServiceProvider.GetRequiredService<IChannelProvider>();
        var queueSetup = scope.ServiceProvider.GetRequiredService<IQueueSetup>();

        var channel = await channelProvider.GetChannelAsync();
        var messageHandler = scope.ServiceProvider.GetRequiredService<IMessageHandler<SampleMessage>>();
        var consumer = await consumerProvider.CreateConsumerAsync(messageHandler);
        var queueProperties = scope.ServiceProvider.GetRequiredService<IOptions<QueueProperties<SampleMessage>>>();
        await queueSetup.CreateQueueAsync(channel, queueProperties.Value);

        await channel.BasicConsumeAsync(
            queue: queueProperties.Value.QueueName,
            autoAck: false,
            consumerTag: $"consumer-{Guid.NewGuid()}",
            noLocal: false,
            exclusive: false,
            arguments: null,
            consumer: consumer,
            cancellationToken: stoppingToken
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(5000, stoppingToken);
        }
    }
}
