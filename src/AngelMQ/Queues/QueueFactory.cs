using AngelMQ.Constants;
using AngelMQ.Properties;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace AngelMQ.Queues;

public sealed class QueueFactory(ILogger<QueueFactory> logger) : IQueueFactory
{
    public async Task CreateAsync<TMessage>(IChannel channel,
                                            QueueProperties<TMessage> queueProperties) where TMessage : class
    {
        if (queueProperties.DeadLetter.Enabled)
            await CreateDeadLetterQueueAsync(channel, queueProperties);

        await CreateMainQueueAsync(channel, queueProperties);

        if (queueProperties.ParkingLot.Enabled)
            await CreateParkingLotQueueAsync(channel, queueProperties);
    }

    private async Task CreateMainQueueAsync<TMessage>(IChannel channel,
                                                      QueueProperties<TMessage> queueProperties)
                                                      where TMessage : class
    {
        var arguments = new Dictionary<string, object?>();
        if (queueProperties.DeadLetter.Enabled)
            arguments.Add(MessageHeaders.DeadLetterExchange, queueProperties.DeadLetterExchangeName);

        await CreateExchangeAsync(channel,
                                  queueProperties.ExchangeName,
                                  queueProperties.ExchangeType);

        await CreateQueueAsync(channel,
                               queueProperties.QueueName,
                               arguments);

        await BindQueueAsync(channel,
                             queueProperties.QueueName,
                             queueProperties.ExchangeName,
                             queueProperties.RoutingKeys);
    }

    private async Task CreateDeadLetterQueueAsync<TMessage>(IChannel channel,
                                                            QueueProperties<TMessage> queueProperties)
                                                            where TMessage : class
    {
        await CreateExchangeAsync(channel,
                                  queueProperties.DeadLetterExchangeName,
                                  queueProperties.ExchangeType);

        await CreateQueueAsync(channel, queueProperties.DeadLetterQueueName, null);

        await BindQueueAsync(channel,
                             queueProperties.DeadLetterQueueName,
                             queueProperties.DeadLetterExchangeName,
                             queueProperties.RoutingKeys);
    }

    private async Task CreateParkingLotQueueAsync<TMessage>(IChannel channel,
                                                            QueueProperties<TMessage> queueProperties)
                                                            where TMessage : class
    {
        var arguments = new Dictionary<string, object?>
        {
            { MessageHeaders.MessageTTL, queueProperties.ParkingLot.TTL },
            { MessageHeaders.DeadLetterExchange, queueProperties.ExchangeName }
        };

        await CreateExchangeAsync(channel,
                                  queueProperties.ParkingLotExchangeName,
                                  queueProperties.ExchangeType);

        await CreateQueueAsync(channel,
                               queueProperties.ParkingLotQueueName,
                               arguments);

        await BindQueueAsync(channel,
                             queueProperties.ParkingLotQueueName,
                             queueProperties.ParkingLotExchangeName,
                             queueProperties.RoutingKeys);
    }

    private async Task CreateQueueAsync(IChannel channel,
                                        string queueName,
                                        IDictionary<string, object?>? arguments = null)
    {
        logger.LogInformation("Creating queue: {QueueName}", queueName);

        await channel.QueueDeclareAsync(queueName,
                                        durable: true,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments);
    }

    private async Task CreateExchangeAsync(IChannel channel, string exchangeName, string exchangeType)
    {
        if (string.IsNullOrWhiteSpace(exchangeName))
            return;

        logger.LogInformation("Creating exchange: {ExchangeName} {ExchangeType}",
                              exchangeName,
                              exchangeType);

        await channel.ExchangeDeclareAsync(exchangeName,
                                           type: exchangeType,
                                           durable: true,
                                           autoDelete: false,
                                           arguments: null);
    }

    private async Task BindQueueAsync(IChannel channel,
                                      string queueName,
                                      string exchangeName,
                                      string[] routingKeys)
    {
        if (string.IsNullOrWhiteSpace(exchangeName))
            return;

        if (routingKeys is null || routingKeys.Length == 0)
            return;

        logger.LogInformation("Binding queue: {QueueName} to exchange: {ExchangeName} with routingKeys: {RoutingKey}",
                              queueName,
                              exchangeName,
                              string.Join(",", routingKeys));

        foreach (var routingKey in routingKeys)
        {
            await channel.QueueBindAsync(queueName,
                                         exchangeName,
                                         routingKey,
                                         arguments: null);
        }
    }
}
