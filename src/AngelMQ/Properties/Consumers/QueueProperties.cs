namespace AngelMQ.Properties.Consumers;

public sealed class QueueProperties<TMessage> where TMessage : class
{
    private const int DefaultPrefetchCount = 250;
    private const int DefaultConsumerCount = 1;

    public string QueueName { get; set; } = string.Empty;
    public ExchangeProperties Exchange { get; set; } = new ExchangeProperties();
    public string[] RoutingKeys { get; set; } = [];
    public ushort PrefetchCount { get; set; } = DefaultPrefetchCount;
    public int ConsumerCount { get; set; } = DefaultConsumerCount;
    public DeadLetterProperties DeadLetter { get; set; } = new DeadLetterProperties();
    public ParkingLotProperties ParkingLot { get; set; } = new ParkingLotProperties();

    public string DeadLetterQueueName => DeadLetter.BuildQueueName(QueueName);
    public string DeadLetterExchangeName => DeadLetter.BuildExchangeName(Exchange.Name);
    public string ParkingLotQueueName => ParkingLot.BuildQueueName(QueueName);
    public string ParkingLotExchangeName => ParkingLot.BuildExchangeName(Exchange.Name);
}
