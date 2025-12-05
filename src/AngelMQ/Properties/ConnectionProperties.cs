using RabbitMQ.Client;

namespace AngelMQ.Properties;

public sealed class ConnectionProperties
{
    private static readonly ConnectionFactory DefaultConnectionFactory = new ConnectionFactory()
    {
        AutomaticRecoveryEnabled = true,
        ConsumerDispatchConcurrency = DefaultConsumerDispatchConcurrency
    };

    private const int DefaultConsumerDispatchConcurrency = 10;
    private const int DefaultMaxRetryAttempts = 5;
    private const int DefaultDelayMultiplier = 2;

    public ConnectionFactory ConnectionFactory { get; private set; } = DefaultConnectionFactory;
    public ushort MaxRetryAttempts { get; set; } = DefaultMaxRetryAttempts;
    public ushort DelayMultiplier { get; set; } = DefaultDelayMultiplier;
    public ChannelPoolProperties ChannelPool { get; set; } = new ChannelPoolProperties();
}
