using AngelMQ.Constants;
using AngelMQ.Properties;
using AngelMQ.Publishers;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AngelMQ.Messages.Errors;

public sealed class MessageErrorHandler(ILogger<MessageErrorHandler> logger,
                                        IMessagePublisher messagePublisher) : IMessageErrorHandler
{
    public async Task HandleAsync<TMessage>(QueueProperties<TMessage> queueProperties,
                                            IChannel messageChannel,
                                            BasicDeliverEventArgs args,
                                            Exception exception) where TMessage : class
    {
        try
        {
            int currentRetryCount = GetRetryCount(args) + 1;
            if (!ShouldRetryMessage(queueProperties, currentRetryCount))
            {
                await SendToDeadLetterAsync(messageChannel, args);
                return;
            }

            await SendToParkingLotAsync(queueProperties.ParkingLotExchangeName,
                                        messageChannel,
                                        args,
                                        exception,
                                        currentRetryCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to handle message error for DeliveryTag: {DeliveryTag}", args.DeliveryTag);
            throw;
        }
    }

    private static int GetRetryCount(BasicDeliverEventArgs args)
    {
        if (args.BasicProperties.Headers is null)
            return 0;

        if (!args.BasicProperties.Headers.TryGetValue(MessageHeaders.Retries, out var retryHeader))
            return 0;

        int retryCount = 0;
        if (retryHeader is not null && int.TryParse(retryHeader.ToString(), out retryCount))
            retryCount = (int)retryHeader;

        return retryCount;
    }

    private static bool ShouldRetryMessage<TMessage>(QueueProperties<TMessage> queueProperties, int currentRetryCount) where TMessage : class =>
        queueProperties.EnableParkingLot && currentRetryCount <= queueProperties.MaxRetryAttempts;

    private async Task SendToDeadLetterAsync(IChannel messageChannel, BasicDeliverEventArgs args)
    {
        logger.LogError("Max retry attempts reached for DeliveryTag: {DeliveryTag}. Sending to dead letter.",
                        args.DeliveryTag);

        await messageChannel.BasicRejectAsync(args.DeliveryTag, requeue: false);
    }

    private async Task SendToParkingLotAsync(string parkingLotExchange, IChannel messageChannel, BasicDeliverEventArgs args, Exception exception, int retryCount)
    {
        logger.LogWarning("Sending message to parking lot after {RetryCount} retries for DeliveryTag: {DeliveryTag}",
                          retryCount, args.DeliveryTag);

        var properties = new BasicProperties
        {
            Headers = args.BasicProperties.Headers ?? new Dictionary<string, object?>()
        };

        properties.Headers[MessageHeaders.Retries] = retryCount;
        properties.Headers[MessageHeaders.ExceptionRootCause] = exception.Message;
        properties.Headers[MessageHeaders.ExceptionStackTrace] = exception.StackTrace;

        await messageChannel.BasicAckAsync(args.DeliveryTag, multiple: false);

        await messagePublisher.PublishAsync(args.Body,
                                            parkingLotExchange,
                                            args.RoutingKey,
                                            properties);
    }
}
