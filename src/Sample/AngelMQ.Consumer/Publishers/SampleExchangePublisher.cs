using AngelMQ.Channels.Pool;
using AngelMQ.Consumer.Listeners.Messages;
using AngelMQ.Properties.Publishers;
using AngelMQ.Publishers;
using AngelMQ.Publishers.Base;
using Microsoft.Extensions.Options;

namespace AngelMQ.Consumer.Publishers;

public sealed class SampleExchangePublisher : ExchangePublisher<SampleMessage>
{
    public SampleExchangePublisher(ILogger<PublisherBase<SampleMessage>> logger,
                                   IChannelPool channelPool,
                                   IMessagePublisher messagePublisher,
                                   IOptions<PublisherProperties<SampleMessage>> options)
                                   : base(logger, channelPool, messagePublisher, options)
    {
    }

    protected override string BuildRoutingKey(SampleMessage message, IDictionary<string, string>? headers = null)
    {
        return $"create.{message.Id}";
    }
}
