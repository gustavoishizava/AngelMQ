using AngelMQ.Channels;
using AngelMQ.Consumers;
using AngelMQ.Properties;
using AngelMQ.Queues;

namespace AngelMQ.Consumer.Listeners;

public sealed class ConsumerTest(IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var consumerProvider = scope.ServiceProvider.GetRequiredService<IConsumerProvider>();
        var channelProvider = scope.ServiceProvider.GetRequiredService<IChannelProvider>();
        var queueSetup = scope.ServiceProvider.GetRequiredService<IQueueSetup>();

        var queueProperties = new QueueProperties
        {
            QueueName = "accounts",
            ExchangeName = "accounts.exchange",
            ExchangeType = "topic",
            RoutingKeys = ["create.#", "update.#"],
            EnableDeadLetter = true,
            EnableParkingLot = true
        };

        var channel = await channelProvider.GetChannelAsync();
        var consumer = await consumerProvider.CreateConsumerAsync();
        await queueSetup.CreateQueueAsync(channel, queueProperties);

        await channel.BasicConsumeAsync(
            queue: queueProperties.QueueName,
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
