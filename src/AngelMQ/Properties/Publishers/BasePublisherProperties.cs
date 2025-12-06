namespace AngelMQ.Properties.Publishers;

public abstract class BasePublisherProperties
{
    public bool Durable { get; set; } = true;
    public bool AutoDelete { get; set; } = false;
    public IDictionary<string, object?>? Arguments { get; set; } = null;
    public bool Passive { get; set; } = false;
    public bool NoWait { get; set; } = false;

    public abstract void Validate();
}
