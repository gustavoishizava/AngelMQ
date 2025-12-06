namespace AngelMQ.Properties.Publishers;

public sealed class PublisherProperties<TMessage> where TMessage : class
{
    public ExchangeProperties? Exchange { get; private set; } = null;
    public QueueProperties? Queue { get; private set; } = null;
    public bool AutoCreate { get; set; } = false;

    public bool IsExchangePublisher => Exchange is not null;
    public bool IsQueuePublisher => Queue is not null;

    public void SetExchange(ExchangeProperties exchangeProperties)
    {
        if (IsQueuePublisher)
            throw new InvalidOperationException("Cannot set Exchange when Queue is already set.");

        Exchange = exchangeProperties;
    }

    public void SetQueue(QueueProperties queueProperties)
    {
        if (IsExchangePublisher)
            throw new InvalidOperationException("Cannot set Queue when Exchange is already set.");

        Queue = queueProperties;
    }
}
