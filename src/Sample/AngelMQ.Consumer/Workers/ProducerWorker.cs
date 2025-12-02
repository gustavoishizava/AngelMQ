using AngelMQ.Consumer.Listeners.Messages;
using AngelMQ.Publishers;

namespace AngelMQ.Consumer.Workers;

public class ProducerWorker : BackgroundService
{
    private readonly ILogger<ProducerWorker> _logger;
    private readonly IMessagePublisher _messagePublisher;

    public ProducerWorker(ILogger<ProducerWorker> logger, IMessagePublisher messagePublisher)
    {
        _logger = logger;
        _messagePublisher = messagePublisher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = Enumerable.Range(0, 50)
                              .Select(_ => PublishAsync(stoppingToken)).ToList();

        await Task.WhenAll(tasks);
    }

    private async Task PublishAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Producing message at: {time}", DateTimeOffset.Now);
            var message = new SampleMessage
            {
                Id = 1
            };

            await _messagePublisher.PublishAsync(message, "accounts.exchange", "create.user");

            await Task.Delay(500, stoppingToken);
        }
    }
}
