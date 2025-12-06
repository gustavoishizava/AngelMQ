namespace AngelMQ.Consumer.Listeners.Messages;

public class QueueMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
