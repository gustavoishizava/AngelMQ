using AngelMQ.Consumer.Listeners.Messages;
using AngelMQ.Messages;
using RabbitMQ.Client.Events;

namespace AngelMQ.Consumer.Listeners.Handlers;

public class SampleMessageHandler(ILogger<SampleMessageHandler> logger) : MessageHandler<SampleMessage>(logger)
{
    protected override Task ProcessAsync(SampleMessage? message, IDictionary<string, string> headers, BasicDeliverEventArgs args)
    {
        logger.LogInformation("Processing SampleMessage with Id: {Id}", message?.Id);
        return Task.CompletedTask;
    }
}
