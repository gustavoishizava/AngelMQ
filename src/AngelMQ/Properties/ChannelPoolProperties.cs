namespace AngelMQ.Properties;

public sealed class ChannelPoolProperties
{
    private const int DefaultMaxPoolSize = 1;
    private const int MinimumPoolSize = 1;

    public int MaxPoolSize { get; private set; } = DefaultMaxPoolSize;

    public void SetMaxPoolSize(int maxPoolSize)
    {
        if (maxPoolSize < MinimumPoolSize)
            throw new ArgumentOutOfRangeException(nameof(maxPoolSize), $"Max pool size must be at least {MinimumPoolSize}.");

        MaxPoolSize = maxPoolSize;
    }
}
