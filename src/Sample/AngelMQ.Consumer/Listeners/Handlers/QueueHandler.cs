using AngelMQ.Consumer.Listeners.Messages;
using AngelMQ.Messages;
using RabbitMQ.Client.Events;

namespace AngelMQ.Consumer.Listeners.Handlers;

public class QueueHandler : MessageHandler<QueueMessage>
{
    private readonly ILogger<MessageHandler<QueueMessage>> _logger;
    public QueueHandler(ILogger<MessageHandler<QueueMessage>> logger) : base(logger)
    {
        _logger = logger;
    }

    protected override Task ProcessAsync(QueueMessage? message, IDictionary<string, string> headers, BasicDeliverEventArgs args)
    {
        _logger.LogInformation("Processing QueueMessage with Id: {Id}", message?.Id);
        return Task.CompletedTask;
    }
}
