using RabbitMQ.Client;

namespace AngelMQ.Publishers;

public interface IMessagePublisher
{
    Task PublishAsync<TMessage>(TMessage message,
                                string exchange,
                                string routingKey,
                                IDictionary<string, string>? headers = null) where TMessage : class;

    Task PublishAsync(ReadOnlyMemory<byte> body,
                      string exchange,
                      string routingKey,
                      BasicProperties? properties = null);
}
