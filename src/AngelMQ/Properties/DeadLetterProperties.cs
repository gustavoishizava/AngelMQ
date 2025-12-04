using AngelMQ.Constants;

namespace AngelMQ.Properties;

public sealed class DeadLetterProperties
{
    public bool Enabled { get; set; } = false;
    public string? QueueName { get; set; }
    public string? QueueSuffix { get; set; } = Suffixes.DeadLetterQueue;
    public string? ExchangeName { get; set; }
    public string? ExchangeSuffix { get; set; } = Suffixes.DeadLetterExchange;

    public string BuildQueueName(string mainQueueName)
    {
        string queueName = string.IsNullOrWhiteSpace(QueueName) ? mainQueueName : QueueName;
        string suffix = string.IsNullOrWhiteSpace(QueueSuffix) ? Suffixes.DeadLetterQueue : QueueSuffix;
        return $"{queueName}{suffix}";
    }

    public string BuildExchangeName(string mainExchangeName)
    {
        string exchangeName = string.IsNullOrWhiteSpace(ExchangeName) ? mainExchangeName : ExchangeName;
        string suffix = string.IsNullOrWhiteSpace(ExchangeSuffix) ? Suffixes.DeadLetterExchange : ExchangeSuffix;
        return $"{exchangeName}{suffix}";
    }
}
