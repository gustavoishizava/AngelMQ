using AngelMQ.Channels;
using AngelMQ.Messages;
using AngelMQ.Properties.Consumers;
using AngelMQ.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AngelMQ.Consumers.Workers;

public sealed class SimpleQueueWorker<TMessage>(
    ILogger<SimpleQueueWorker<TMessage>> logger,
    IServiceScopeFactory serviceScopeFactory,
    IQueueFactory queueFactory,
    IConsumerFactory consumerFactory,
    IOptions<QueueProperties<TMessage>> options)
    : BackgroundService
    where TMessage : class
{
    private readonly QueueProperties<TMessage> _queueProperties = options.Value;
    private const int DelayIntervalMs = 5000;
    private string _messageName = typeof(TMessage).Name ?? "UnknownMessage";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting consumer worker for message type {MessageType}", _messageName);

        if (_queueProperties.ConsumerCount <= 0)
        {
            logger.LogWarning("No consumers configured for message type {MessageType}", _messageName);
            return;
        }

        await SetupAsync();

        var consumerTasks = Enumerable.Range(0, _queueProperties.ConsumerCount)
                                          .Select(_ =>
                                            StartConsumerAsync(stoppingToken))
                                          .ToList();

        await Task.WhenAll(consumerTasks);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping consumer worker for message type {MessageType}", _messageName);
        return base.StopAsync(cancellationToken);
    }

    private async Task SetupAsync()
    {
        using var scope = serviceScopeFactory.CreateScope();
        var channelProvider = scope.ServiceProvider.GetRequiredService<IChannelProvider>();

        var channel = await channelProvider.GetAsync();

        await queueFactory.CreateAsync(channel, _queueProperties);
        await channelProvider.CloseAsync();
    }

    private async Task StartConsumerAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var messageHandler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TMessage>>();
        var channelProvider = scope.ServiceProvider.GetRequiredService<IChannelProvider>();
        var channel = await channelProvider.GetAsync(_queueProperties.PrefetchCount);
        var consumer = consumerFactory.Create(channel, messageHandler, _queueProperties);

        await channel.BasicConsumeAsync(queue: _queueProperties.QueueName,
                                        autoAck: false,
                                        consumerTag: $"consumer-{Guid.NewGuid()}",
                                        noLocal: false,
                                        exclusive: false,
                                        arguments: null,
                                        consumer: consumer,
                                        cancellationToken: cancellationToken);

        logger.LogInformation("Started consuming messages from queue {QueueName}", _queueProperties.QueueName);

        while (!cancellationToken.IsCancellationRequested)
        {
            logger.LogDebug("Consumer on queue {QueueName} is active", _queueProperties.QueueName);
            await Task.Delay(DelayIntervalMs, cancellationToken);
        }

        await channelProvider.CloseAsync();
        logger.LogInformation("Stopped consuming messages from queue {QueueName}", _queueProperties.QueueName);
    }
}
