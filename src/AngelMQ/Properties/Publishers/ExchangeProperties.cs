using RabbitMQ.Client;

namespace AngelMQ.Properties.Publishers;

public sealed class ExchangeProperties : BasePublisherProperties
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = ExchangeType.Topic;

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Exchange name cannot be null or whitespace.", nameof(Name));

        if (string.IsNullOrWhiteSpace(Type))
            throw new ArgumentException("Exchange type cannot be null or whitespace.", nameof(Type));
    }
}
