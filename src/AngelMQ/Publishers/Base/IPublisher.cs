namespace AngelMQ.Publishers.Base;

public interface IPublisher<TMessage> where TMessage : class
{
    Task PublishAsync(TMessage message, IDictionary<string, string>? headers = null);
}
