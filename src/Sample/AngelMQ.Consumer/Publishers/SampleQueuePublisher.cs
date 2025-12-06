using AngelMQ.Channels.Pool;
using AngelMQ.Consumer.Listeners.Messages;
using AngelMQ.Properties.Publishers;
using AngelMQ.Publishers;
using AngelMQ.Publishers.Base;
using Microsoft.Extensions.Options;

namespace AngelMQ.Consumer.Publishers;

public class SampleQueuePublisher : QueuePublisher<QueueMessage>
{
    public SampleQueuePublisher(ILogger<PublisherBase<QueueMessage>> logger,
                                IChannelPool channelPool,
                                IMessagePublisher messagePublisher,
                                IOptions<PublisherProperties<QueueMessage>> options)
                                : base(logger, channelPool, messagePublisher, options)
    {
    }
}
