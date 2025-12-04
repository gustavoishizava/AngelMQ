using System.Security.Authentication;

namespace AngelMQ.Properties;

public sealed class ConnectionProperties
{
    private const int DefaultMaxPoolSize = 1;
    private const int MinimumPoolSize = 1;
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
    public int MaxChannelPoolSize { get; private set; } = DefaultMaxPoolSize;
    public int ChannelPoolTimeout { get; set; } = Timeout.Infinite;
    public bool EnableSsl { get; set; } = false;
    public SslProtocols? SslProtocols { get; set; }

    public void SetMaxChannelPoolSize(int maxPoolSize)
    {
        if (maxPoolSize < MinimumPoolSize)
            throw new ArgumentOutOfRangeException(nameof(maxPoolSize), $"Max pool size must be at least {MinimumPoolSize}.");

        MaxChannelPoolSize = maxPoolSize;
    }
}
