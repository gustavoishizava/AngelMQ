using AngelMQ.Constants;

namespace AngelMQ.Properties;

public sealed class QueueProperties<TMessage> where TMessage : class
{
    private const int DefaultPrefetchCount = 250;
    private const int DefaultConsumerCount = 1;

    public string QueueName { get; set; } = string.Empty;
    public string ExchangeName { get; set; } = string.Empty;
    public string ExchangeType { get; set; } = string.Empty;
    public string[] RoutingKeys { get; set; } = [];
    public ushort PrefetchCount { get; set; } = DefaultPrefetchCount;
    public int ConsumerCount { get; set; } = DefaultConsumerCount;
    public DeadLetterProperties DeadLetter { get; private set; } = new DeadLetterProperties();
    public ParkingLotProperties ParkingLot { get; private set; } = new ParkingLotProperties();

    public string DeadLetterQueueName => DeadLetter.BuildQueueName(QueueName);
    public string DeadLetterExchangeName => DeadLetter.BuildExchangeName(ExchangeName);
    public string ParkingLotQueueName => ParkingLot.BuildQueueName(QueueName);
    public string ParkingLotExchangeName => ParkingLot.BuildExchangeName(ExchangeName);
}
