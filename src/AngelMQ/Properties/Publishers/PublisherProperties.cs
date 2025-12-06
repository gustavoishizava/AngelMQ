namespace AngelMQ.Properties.Publishers;

public class PublisherProperties<TMessage, TPublishProps>
    where TMessage : class
    where TPublishProps : BasePublisherProperties, new()
{
    public TPublishProps Configuration { get; private set; } = new TPublishProps();
    public bool AutoCreate { get; set; } = true;

    public bool IsExchangePublisher => Configuration is ExchangeProperties;
    public bool IsQueuePublisher => Configuration is QueueProperties;
}
