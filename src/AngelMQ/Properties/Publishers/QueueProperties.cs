namespace AngelMQ.Properties.Publishers;

public sealed class QueueProperties : BasePublisherProperties
{
    public string Name { get; set; } = string.Empty;
    public bool Exclusive { get; set; } = false;

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidOperationException("Queue name cannot be null or empty.");
    }
}
