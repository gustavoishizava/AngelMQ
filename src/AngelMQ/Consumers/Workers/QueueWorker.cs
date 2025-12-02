using AngelMQ.Channels;
using AngelMQ.Messages;
using AngelMQ.Properties;
using AngelMQ.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        var queueProperties = scope.ServiceProvider.GetRequiredService<IOptions<QueueProperties<TMessage>>>();

        if (queueProperties.Value.ConsumerCount <= 0)
        {
            logger.LogWarning("No consumers configured for message type {MessageType}", _messageName);
            return;
        }

        await SetupAsync(queueProperties.Value);

        var consumerTasks = Enumerable.Range(0, queueProperties.Value.ConsumerCount)
                                          .Select(_ =>
                                            StartConsumerAsync(queueProperties.Value.QueueName,
                                                               queueProperties.Value.PrefetchCount,
                                                               stoppingToken))
                                          .ToList();

        await Task.WhenAll(consumerTasks);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping consumer worker for message type {MessageType}", _messageName);
        return base.StopAsync(cancellationToken);
    }

    private async Task SetupAsync(QueueProperties<TMessage> queueProperties)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var channelProvider = scope.ServiceProvider.GetRequiredService<IChannelProvider>();
        var queueSetup = scope.ServiceProvider.GetRequiredService<IQueueSetup>();

        var channel = await channelProvider.GetChannelAsync();

        await queueSetup.CreateQueueAsync(channel, queueProperties);
        await channelProvider.CloseChannelAsync();
    }

    private async Task StartConsumerAsync(string queueName,
                                          ushort prefetchCount,
                                          CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var consumerProvider = scope.ServiceProvider.GetRequiredService<IConsumerProvider>();

        var messageHandler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TMessage>>();
        var consumer = await consumerProvider.CreateConsumerAsync(messageHandler, prefetchCount);

        await consumer.Channel.BasicConsumeAsync(queue: queueName,
                                                 autoAck: false,
                                                 consumerTag: $"consumer-{Guid.NewGuid()}",
                                                 noLocal: false,
                                                 exclusive: false,
                                                 arguments: null,
                                                 consumer: consumer,
                                                 cancellationToken: cancellationToken);

        logger.LogInformation("Started consuming messages from queue {QueueName}", queueName);

        while (!cancellationToken.IsCancellationRequested)
        {
            logger.LogDebug("Consumer on queue {QueueName} is active", queueName);
            await Task.Delay(DelayIntervalMs, cancellationToken);
        }
    }
}
