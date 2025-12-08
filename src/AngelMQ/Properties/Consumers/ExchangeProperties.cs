using RabbitMQ.Client;

namespace AngelMQ.Properties.Consumers;

public sealed class ExchangeProperties
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = ExchangeType.Topic;
    public bool AutoCreate { get; set; } = true;
}
