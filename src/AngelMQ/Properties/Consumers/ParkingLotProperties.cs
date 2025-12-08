using AngelMQ.Constants;

namespace AngelMQ.Properties.Consumers;

public sealed class ParkingLotProperties
{
    private const int DefaultParkingLotTTL = 60000;
    private const int MinimumParkingLotTTL = 1000;
    private const ushort DefaultMaxRetryAttempts = 3;
    private const ushort MinimumRetryAttempts = 1;

    public bool Enabled { get; set; } = false;
    public string? QueueName { get; set; }
    public string? QueueSuffix { get; set; } = Suffixes.ParkingLotQueue;
    public string? ExchangeName { get; set; }
    public string? ExchangeSuffix { get; set; } = Suffixes.ParkingLotExchange;
    public int TTL { get; private set; } = DefaultParkingLotTTL;
    public ushort MaxRetryAttempts { get; private set; } = DefaultMaxRetryAttempts;

    public string BuildQueueName(string mainQueueName)
    {
        string queueName = string.IsNullOrWhiteSpace(QueueName) ? mainQueueName : QueueName;
        string suffix = string.IsNullOrWhiteSpace(QueueSuffix) ? Suffixes.ParkingLotQueue : QueueSuffix;
        return $"{queueName}{suffix}";
    }

    public string BuildExchangeName(string mainExchangeName)
    {
        string exchangeName = string.IsNullOrWhiteSpace(ExchangeName) ? mainExchangeName : ExchangeName;
        string suffix = string.IsNullOrWhiteSpace(ExchangeSuffix) ? Suffixes.ParkingLotExchange : ExchangeSuffix;
        return $"{exchangeName}{suffix}";
    }

    public void SetTTL(int ttl)
    {
        if (ttl < MinimumParkingLotTTL)
            throw new ArgumentOutOfRangeException(nameof(ttl), $"Parking lot TTL must be at least {MinimumParkingLotTTL} milliseconds.");

        TTL = ttl;
    }

    public void SetMaxRetryAttempts(ushort maxRetryAttempts)
    {
        if (maxRetryAttempts < MinimumRetryAttempts)
            throw new ArgumentOutOfRangeException(nameof(maxRetryAttempts), $"Max retry attempts must be at least {MinimumRetryAttempts}.");

        MaxRetryAttempts = maxRetryAttempts;
    }
}
