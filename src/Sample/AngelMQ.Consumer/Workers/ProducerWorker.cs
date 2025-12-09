using AngelMQ.Consumer.Listeners.Messages;
using AngelMQ.Publishers;
using AngelMQ.Publishers.Base;

namespace AngelMQ.Consumer.Workers;

public class ProducerWorker : BackgroundService
{
    private readonly ILogger<ProducerWorker> _logger;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IPublisher<SampleMessage> _samplePublisher;
    private readonly IPublisher<QueueMessage> _queuePublisher;

    public ProducerWorker(ILogger<ProducerWorker> logger, IMessagePublisher messagePublisher, IPublisher<SampleMessage> samplePublisher, IPublisher<QueueMessage> queuePublisher)
    {
        _logger = logger;
        _messagePublisher = messagePublisher;
        _samplePublisher = samplePublisher;
        _queuePublisher = queuePublisher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = Enumerable.Range(0, 10)
                              .Select(_ => PublishAsync(stoppingToken)).ToList();

        await Task.WhenAll(tasks);
    }

    private async Task PublishAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Producing message at: {time}", DateTimeOffset.Now);

            var task1 = _samplePublisher.PublishAsync(new SampleMessage
            {
                Id = Random.Shared.Next(1, 1000),
                Country = "br"
            });

            var task2 = _samplePublisher.PublishAsync(new SampleMessage
            {
                Id = Random.Shared.Next(1, 1000),
                Country = "mx"
            });

            var task3 = _queuePublisher.PublishAsync(new QueueMessage
            {
                Id = Random.Shared.Next(1, 1000)
            });

            await Task.WhenAll(task1, task2, task3);

            await Task.Delay(500, stoppingToken);
        }
    }
}
