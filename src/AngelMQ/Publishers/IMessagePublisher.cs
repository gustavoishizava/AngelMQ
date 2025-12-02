namespace AngelMQ.Publishers;

public interface IMessagePublisher
{
    Task PublishAsync<TMessage>(TMessage message,
                                string exchange,
                                string routingKey,
                                IDictionary<string, string>? headers = null) where TMessage : class;
}
