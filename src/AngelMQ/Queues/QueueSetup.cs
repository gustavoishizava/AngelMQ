using AngelMQ.Constants;
using AngelMQ.Properties;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace AngelMQ.Queues;

public sealed class QueueSetup(ILogger<QueueSetup> logger) : IQueueSetup
{
    public async Task CreateQueueAsync<TMessage>(IChannel channel,
                                       QueueProperties<TMessage> queueProperties) where TMessage : class
    {
        if (queueProperties.EnableDeadLetter)
            await SetupDeadLetterQueueAsync(channel, queueProperties);

        await SetupMainQueueAsync(channel, queueProperties);

        if (queueProperties.EnableParkingLot)
            await SetupParkingLotQueueAsync(channel, queueProperties);
    }

    private async Task SetupMainQueueAsync<TMessage>(IChannel channel,
                                                     QueueProperties<TMessage> queueProperties)
                                                     where TMessage : class
    {
        var arguments = new Dictionary<string, object?>();
        if (queueProperties.EnableDeadLetter)
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

    private async Task SetupDeadLetterQueueAsync<TMessage>(IChannel channel,
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

    private async Task SetupParkingLotQueueAsync<TMessage>(IChannel channel,
                                                           QueueProperties<TMessage> queueProperties)
                                                           where TMessage : class
    {
        var arguments = new Dictionary<string, object?>
        {
            { MessageHeaders.MessageTTL, queueProperties.ParkingLotTTL },
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

    private Task CreateQueueAsync(IChannel channel,
                                  string queueName,
                                  IDictionary<string, object?>? arguments = null)
    {
        logger.LogInformation("Creating queue: {QueueName}", queueName);

        return channel.QueueDeclareAsync(queueName,
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments);
    }

    private Task CreateExchangeAsync(IChannel channel, string exchangeName, string exchangeType)
    {
        logger.LogInformation("Creating exchange: {ExchangeName} {ExchangeType}",
                              exchangeName,
                              exchangeType);

        return channel.ExchangeDeclareAsync(exchangeName,
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
