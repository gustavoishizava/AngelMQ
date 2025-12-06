using System.Net.Mime;
using System.Text;
using System.Text.Json;
using AngelMQ.Channels.Pool;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace AngelMQ.Publishers;

public sealed class MessagePublisher(ILogger<MessagePublisher> logger,
                                     IChannelPool channelPool) : IMessagePublisher
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = false
    };

    public async Task PublishAsync<TMessage>(TMessage message,
                                             string exchange,
                                             string routingKey,
                                             IDictionary<string, string>? headers = null) where TMessage : class
    {
        logger.LogDebug("Publishing message to exchange '{Exchange}' with routing key '{RoutingKey}'", exchange, routingKey);

        var channel = await channelPool.GetAsync();

        try
        {
            var body = GetBody(message);
            var properties = GetProperties(headers);

            await channel.BasicPublishAsync(exchange,
                                            routingKey,
                                            false,
                                            properties,
                                            body);

            logger.LogDebug("Message published to exchange '{Exchange}' with routing key '{RoutingKey}'", exchange, routingKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing message to exchange '{Exchange}' with routing key '{RoutingKey}'", exchange, routingKey);
            throw;
        }
        finally
        {
            await channelPool.ReturnAsync(channel);
        }
    }

    public async Task PublishAsync(ReadOnlyMemory<byte> body, string exchange, string routingKey, BasicProperties? properties = null)
    {
        logger.LogDebug("Publishing message to exchange '{Exchange}' with routing key '{RoutingKey}'", exchange, routingKey);

        var channel = await channelPool.GetAsync();

        try
        {
            await channel.BasicPublishAsync(exchange,
                                            routingKey,
                                            false,
                                            properties ?? GetProperties(null),
                                            body);

            logger.LogDebug("Message published to exchange '{Exchange}' with routing key '{RoutingKey}'", exchange, routingKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing message to exchange '{Exchange}' with routing key '{RoutingKey}'", exchange, routingKey);
            throw;
        }
        finally
        {
            await channelPool.ReturnAsync(channel);
        }
    }

    private static ReadOnlyMemory<byte> GetBody<TMessage>(TMessage message) where TMessage : class =>
        JsonSerializer.SerializeToUtf8Bytes(message, _jsonSerializerOptions);

    private static BasicProperties GetProperties(IDictionary<string, string>? headers)
    {
        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = MediaTypeNames.Application.Json,
            DeliveryMode = DeliveryModes.Persistent,
            ContentEncoding = Encoding.UTF8.WebName,
            MessageId = Guid.NewGuid().ToString()
        };

        if (headers is not null)
        {
            properties.Headers ??= new Dictionary<string, object?>();
            foreach (var (key, value) in headers)
            {
                properties.Headers[key] = value;
            }
        }

        return properties;
    }
}