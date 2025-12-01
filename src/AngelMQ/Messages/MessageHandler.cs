using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AngelMQ.Messages;

public abstract class MessageHandler<TMessage>(ILogger<MessageHandler<TMessage>> logger) : IMessageHandler<TMessage> where TMessage : class
{
    private readonly JsonSerializerOptions _defaultSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        IgnoreReadOnlyProperties = true
    };

    public async Task HandleAsync(BasicDeliverEventArgs args)
    {
        try
        {
            TMessage? message = DeserializeMessage(args.Body);
            IDictionary<string, string> headers = ExtractHeaders(args.BasicProperties);
            await ProcessAsync(message, headers, args);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message with DeliveryTag {DeliveryTag}", args.DeliveryTag);
        }
    }

    protected abstract Task ProcessAsync(TMessage? message,
                                         IDictionary<string, string> headers,
                                         BasicDeliverEventArgs args);

    protected virtual JsonSerializerOptions GetSerializerOptions() =>
        _defaultSerializerOptions;

    private TMessage? DeserializeMessage(ReadOnlyMemory<byte> body)
    {
        return JsonSerializer.Deserialize<TMessage>(body.Span,
                                                    GetSerializerOptions());
    }

    private IDictionary<string, string> ExtractHeaders(IReadOnlyBasicProperties properties)
    {
        var headers = new Dictionary<string, string>();
        if (properties.Headers is null)
            return headers;

        foreach (var header in properties.Headers)
        {
            if (header.Value is byte[] byteArray)
                headers[header.Key] = System.Text.Encoding.UTF8.GetString(byteArray);
            else
                headers[header.Key] = header.Value?.ToString() ?? string.Empty;
        }

        return headers;
    }
}
