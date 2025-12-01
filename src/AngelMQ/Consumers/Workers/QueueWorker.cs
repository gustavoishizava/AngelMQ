using AngelMQ.Channels;
using AngelMQ.Messages;
using AngelMQ.Properties;
using AngelMQ.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AngelMQ.Consumers.Workers;

public sealed class QueueWorker<TMessage>(ILogger<QueueWorker<TMessage>> logger,
                                          IServiceScopeFactory serviceScopeFactory) : BackgroundService
    where TMessage : class
{
    private const int DelayIntervalMs = 5000;
    private string _messageName = typeof(TMessage).Name ?? "UnknownMessage";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting consumer worker for message type {MessageType}", _messageName);

        using var scope = serviceScopeFactory.CreateScope();
        var channelProvider = scope.ServiceProvider.GetRequiredService<IChannelProvider>();
        var consumerProvider = scope.ServiceProvider.GetRequiredService<IConsumerProvider>();
        var queueSetup = scope.ServiceProvider.GetRequiredService<IQueueSetup>();
        var messageHandler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TMessage>>();
        var queueProperties = scope.ServiceProvider.GetRequiredService<IOptions<QueueProperties<TMessage>>>();

        var channel = await channelProvider.GetChannelAsync();
        await queueSetup.CreateQueueAsync(channel, queueProperties.Value);

        var consumer = await consumerProvider.CreateConsumerAsync(messageHandler);
        await StartConsumerAsync(channel, consumer, queueProperties.Value.QueueName, stoppingToken);

        logger.LogInformation("Consumer worker for message type {MessageType} is running", _messageName);

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogDebug("Consumer worker for message type {MessageType} is alive", _messageName);
            await Task.Delay(DelayIntervalMs, stoppingToken);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping consumer worker for message type {MessageType}", _messageName);
        return base.StopAsync(cancellationToken);
    }

    private async Task StartConsumerAsync(IChannel channel,
                                          AsyncDefaultBasicConsumer consumer,
                                          string queueName,
                                          CancellationToken cancellationToken)
    {
        await channel.BasicConsumeAsync(queue: queueName,
                                        autoAck: false,
                                        consumerTag: $"consumer-{Guid.NewGuid()}",
                                        noLocal: false,
                                        exclusive: false,
                                        arguments: null,
                                        consumer: consumer,
                                        cancellationToken: cancellationToken);
    }
}
