using RabbitMQ.Client;

namespace AngelMQ.Properties;

public sealed class ConnectionProperties
{
    private const int DefaultConsumerDispatchConcurrency = 1;
    private const int DefaultMaxRetryAttempts = 5;
    private const int DefaultDelayMultiplier = 2;

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public ushort MaxRetryAttempts { get; set; } = DefaultMaxRetryAttempts;
    public ushort DelayMultiplier { get; set; } = DefaultDelayMultiplier;
    public ushort ConsumerDispatchConcurrency { get; set; } = DefaultConsumerDispatchConcurrency;
    public ChannelPoolProperties ChannelPool { get; set; } = new ChannelPoolProperties();
    public SslOption? Ssl { get; set; } = null;
}
