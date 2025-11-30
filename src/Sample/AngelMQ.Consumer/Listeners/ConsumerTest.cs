using AngelMQ.Connection;

namespace AngelMQ.Consumer.Listeners;

public sealed class ConsumerTest(IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var connection = scope.ServiceProvider.GetRequiredService<IRabbitMQConnectionProvider>();

        while (!stoppingToken.IsCancellationRequested)
        {
            var conn = await connection.GetConnectionAsync();
            Console.WriteLine($"Connection State: {conn.IsOpen}");
            await Task.Delay(5000, stoppingToken);
        }
    }
}
