using AngelMQ.Channels;

namespace AngelMQ.Consumer.Listeners;

public sealed class ConsumerTest(IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var channelProvider = scope.ServiceProvider.GetRequiredService<IChannelProvider>();

        while (!stoppingToken.IsCancellationRequested)
        {
            await CreateQueue(channelProvider);
            await Task.Delay(5000, stoppingToken);
        }
    }

    private async Task CreateQueue(IChannelProvider channelProvider)
    {
        var channel = await channelProvider.GetChannelAsync(10);

        await channel.QueueDeclareAsync(queue: "test-queue",
                                        durable: false,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);
    }
}
