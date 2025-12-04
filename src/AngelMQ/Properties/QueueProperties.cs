using AngelMQ.Constants;

namespace AngelMQ.Properties;

public sealed class QueueProperties<TMessage> where TMessage : class
{
    private const int DefaultPrefetchCount = 250;
    private const ushort DefaultMaxRetryAttempts = 3;
    private const int DefaultParkingLotTTL = 60000;
    private const int DefaultConsumerCount = 1;

    public string QueueName { get; set; } = string.Empty;
    public string ExchangeName { get; set; } = string.Empty;
    public string ExchangeType { get; set; } = string.Empty;
    public string[] RoutingKeys { get; set; } = [];
    public bool EnableDeadLetter { get; set; }
    public bool EnableParkingLot { get; set; }
    public int ParkingLotTTL { get; set; } = DefaultParkingLotTTL;
    public int ConsumerCount { get; set; } = DefaultConsumerCount;
    public ushort PrefetchCount { get; set; } = DefaultPrefetchCount;
    public ushort MaxRetryAttempts { get; set; } = DefaultMaxRetryAttempts;

    public string DeadLetterQueueName => $"{QueueName}.{QueueSuffix.DeadLetterQueue}";
    public string DeadLetterExchangeName => $"{ExchangeName}.{QueueSuffix.DeadLetterExchange}";
    public string ParkingLotQueueName => $"{QueueName}.{QueueSuffix.ParkingLotQueue}";
    public string ParkingLotExchangeName => $"{ExchangeName}.{QueueSuffix.ParkingLotExchange}";
}
