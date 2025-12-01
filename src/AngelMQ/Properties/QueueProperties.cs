using AngelMQ.Constants;

namespace AngelMQ.Properties;

public sealed class QueueProperties<TMessage> where TMessage : class
{
    public string QueueName { get; set; } = string.Empty;
    public string ExchangeName { get; set; } = string.Empty;
    public string ExchangeType { get; set; } = string.Empty;
    public string[] RoutingKeys { get; set; } = [];
    public bool EnableDeadLetter { get; set; }
    public bool EnableParkingLot { get; set; }
    public int ParkingLotTTL { get; set; } = 60000;

    public string DeadLetterQueueName => $"{QueueName}.{QueueSuffix.DeadLetterQueue}";
    public string DeadLetterExchangeName => $"{ExchangeName}.{QueueSuffix.DeadLetterExchange}";
    public string ParkingLotQueueName => $"{QueueName}.{QueueSuffix.ParkingLotQueue}";
    public string ParkingLotExchangeName => $"{ExchangeName}.{QueueSuffix.ParkingLotExchange}";
}
