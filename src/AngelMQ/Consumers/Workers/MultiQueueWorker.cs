using AngelMQ.Channels;
using AngelMQ.Consumers.Workers.Abstractions;
using AngelMQ.Messages;
using AngelMQ.Properties.Consumers;
using AngelMQ.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AngelMQ.Consumers.Workers;

public sealed class MultiQueueWorker<TMessage>(
    ILogger<MultiQueueWorker<TMessage>> logger,
    IServiceScopeFactory serviceScopeFactory,
    IQueueFactory queueFactory,
    IConsumerFactory consumerFactory,
    IQueueProvider<TMessage> queueProvider) : BackgroundService
    where TMessage : class
{
    private const int DelayIntervalMs = 5000;
    private string _messageName = typeof(TMessage).Name ?? "UnknownMessage";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting consumer worker for message type {MessageType}", _messageName);

        var queueProperties = await queueProvider.GetQueuePropertiesAsync();
        if (queueProperties is null || queueProperties.Count == 0)
        {
            logger.LogWarning("No queues configured for message type {MessageType}", _messageName);
            return;
        }

        var queuesTask = queueProperties
            .Select(qp => StartQueueAsync(qp, stoppingToken))
            .ToList();

        await Task.WhenAll(queuesTask);
    }

    private async Task StartQueueAsync(QueueProperties<TMessage> queueProperties, CancellationToken cancellationToken)
    {
        logger.LogInformation("Setting up queue {QueueName} for message type {MessageType}", queueProperties.QueueName, _messageName);

        await SetupAsync(queueProperties);

        var consumerTasks = Enumerable.Range(0, queueProperties.ConsumerCount)
                                          .Select(_ =>
                                            StartConsumerAsync(queueProperties, cancellationToken))
                                          .ToList();

        await Task.WhenAll(consumerTasks);
    }

    private async Task SetupAsync(QueueProperties<TMessage> queueProperties)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var channelProvider = scope.ServiceProvider.GetRequiredService<IChannelProvider>();

        var channel = await channelProvider.GetAsync();

        await queueFactory.CreateAsync(channel, queueProperties);
        await channelProvider.CloseAsync();
    }

    private async Task StartConsumerAsync(QueueProperties<TMessage> queueProperties, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var messageHandler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TMessage>>();
        var channelProvider = scope.ServiceProvider.GetRequiredService<IChannelProvider>();
        var channel = await channelProvider.GetAsync(queueProperties.PrefetchCount);
        var consumer = consumerFactory.Create(channel, messageHandler, queueProperties);

        await channel.BasicConsumeAsync(queue: queueProperties.QueueName,
                                        autoAck: false,
                                        consumerTag: $"consumer-{Guid.NewGuid()}",
                                        noLocal: false,
                                        exclusive: false,
                                        arguments: null,
                                        consumer: consumer,
                                        cancellationToken: cancellationToken);

        logger.LogInformation("Started consuming messages from queue {QueueName}", queueProperties.QueueName);

        while (!cancellationToken.IsCancellationRequested)
        {
            logger.LogDebug("Consumer on queue {QueueName} is active", queueProperties.QueueName);
            await Task.Delay(DelayIntervalMs, cancellationToken);
        }

        await channelProvider.CloseAsync();
        logger.LogInformation("Stopped consuming messages from queue {QueueName}", queueProperties.QueueName);
    }
}
