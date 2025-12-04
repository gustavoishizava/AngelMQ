namespace AngelMQ.Properties;

public sealed class ChannelPoolProperties
{
    private const int DefaultMaxPoolSize = 1;
    private const int MinimumPoolSize = 1;

    public int MaxSize { get; private set; } = DefaultMaxPoolSize;
    public int Timeout { get; private set; } = System.Threading.Timeout.Infinite;

    public void SetMaxSize(int maxSize)
    {
        if (maxSize < MinimumPoolSize)
            throw new ArgumentOutOfRangeException(nameof(maxSize), $"Max pool size must be at least {MinimumPoolSize}.");

        MaxSize = maxSize;
    }

    public void SetTimeout(int timeout)
    {
        if (timeout < System.Threading.Timeout.Infinite)
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be greater than or equal to -1.");

        Timeout = timeout;
    }
}
