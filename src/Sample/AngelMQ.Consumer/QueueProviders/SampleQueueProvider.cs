using AngelMQ.Consumer.Listeners.Messages;
using AngelMQ.Consumers.Workers.Abstractions;
using AngelMQ.Properties.Consumers;

namespace AngelMQ.Consumer.QueueProviders;

public sealed class SampleQueueProvider : IQueueProvider<SampleMessage>
{
    public Task<List<QueueProperties<SampleMessage>>> GetQueuePropertiesAsync()
    {
        var br = new QueueProperties<SampleMessage>
        {
            QueueName = "br.accounts",
            Exchange = new ExchangeProperties
            {
                Name = "accounts.exchange",
                Type = "topic",
                AutoCreate = false
            },
            RoutingKeys = ["br.*"],
            ConsumerCount = 2,
            PrefetchCount = 250
        };
        br.DeadLetter.Enabled = true;
        br.ParkingLot.Enabled = true;

        var mx = new QueueProperties<SampleMessage>
        {
            QueueName = "mx.accounts",
            Exchange = new ExchangeProperties
            {
                Name = "accounts.exchange",
                Type = "topic",
                AutoCreate = false
            },
            RoutingKeys = ["mx.*"],
            ConsumerCount = 1,
            PrefetchCount = 250
        };
        mx.DeadLetter.Enabled = true;
        mx.ParkingLot.Enabled = true;

        return Task.FromResult(new List<QueueProperties<SampleMessage>>
        {
            br,
            mx
        });
    }
}
