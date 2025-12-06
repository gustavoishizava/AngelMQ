namespace AngelMQ.Properties.Publishers;

public sealed class QueueProperties
{
    public string Name { get; set; } = string.Empty;
    public bool Durable { get; set; } = true;
    public bool Exclusive { get; set; } = false;
    public bool AutoDelete { get; set; } = false;
    public IDictionary<string, object?>? Arguments { get; set; } = null;
    public bool Passive { get; set; } = false;
    public bool NoWait { get; set; } = false;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidOperationException("Queue name cannot be null or empty.");
    }
}
