using RabbitMQ.Client;

namespace AngelMQ.Properties.Publishers;

public sealed class ExchangeProperties
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = ExchangeType.Topic;
    public bool Durable { get; set; } = true;
    public bool AutoDelete { get; set; } = false;
    public IDictionary<string, object?>? Arguments { get; set; } = null;
    public bool Passive { get; set; } = false;
    public bool NoWait { get; set; } = false;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Exchange name cannot be null or whitespace.", nameof(Name));

        if (string.IsNullOrWhiteSpace(Type))
            throw new ArgumentException("Exchange type cannot be null or whitespace.", nameof(Type));
    }
}
