using AngelMQ.Consumers;
using AngelMQ.Messages;
using AngelMQ.Properties;
using Moq;
using Moq.AutoMock;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AngelMQ.UnitTests.Consumers;

public class ConsumerFactoryTests
{
    private readonly AutoMocker _mocker;
    private readonly Mock<IChannel> _channelMock;
    private readonly QueueProperties<TestMessage> _queueProperties;

    public ConsumerFactoryTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<IMessageHandler<TestMessage>>(new TestHandler());
        _channelMock = new Mock<IChannel>();
        _queueProperties = new QueueProperties<TestMessage>
        {
            QueueName = "test-queue",
            ExchangeName = "test-exchange"
        };
    }

    [Fact]
    public void Create_ShouldReturnNewConsumer()
    {
        // Arrange
        var factory = _mocker.CreateInstance<ConsumerFactory>();

        // Act
        var result = factory.Create(_channelMock.Object, _mocker.Get<IMessageHandler<TestMessage>>(), _queueProperties);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<AsyncEventingBasicConsumer>(result);
        Assert.Equal(_channelMock.Object, result.Channel);
    }

    [Fact]
    public void Create_WhenCalledMultipleTimes_ShouldReturnDifferentInstances()
    {
        // Arrange
        var factory = _mocker.CreateInstance<ConsumerFactory>();

        // Act
        var result1 = factory.Create(_channelMock.Object, _mocker.Get<IMessageHandler<TestMessage>>(), _queueProperties);
        var result2 = factory.Create(_channelMock.Object, _mocker.Get<IMessageHandler<TestMessage>>(), _queueProperties);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotSame(result1, result2);
    }

    [Fact]
    public void Create_ShouldCreateConsumerWithCorrectChannel()
    {
        // Arrange
        var factory = _mocker.CreateInstance<ConsumerFactory>();

        // Act
        var consumer = factory.Create(_channelMock.Object, _mocker.Get<IMessageHandler<TestMessage>>(), _queueProperties) as AsyncEventingBasicConsumer;

        // Assert
        Assert.NotNull(consumer);
        Assert.Equal(_channelMock.Object, consumer.Channel);
    }

    private class TestMessage
    {
        public int Id { get; set; }
    }

    private class TestHandler : IMessageHandler<TestMessage>
    {
        public Task HandleAsync(BasicDeliverEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
