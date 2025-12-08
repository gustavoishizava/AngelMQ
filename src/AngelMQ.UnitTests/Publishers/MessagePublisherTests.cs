using System.Text;
using AngelMQ.Channels.Pool;
using AngelMQ.Publishers;
using Moq;
using Moq.AutoMock;
using RabbitMQ.Client;

namespace AngelMQ.UnitTests.Publishers;

public class MessagePublisherTests
{
    private readonly AutoMocker _mocker;
    private readonly Mock<IChannelPool> _channelPoolMock;
    private readonly Mock<IChannel> _channelMock;

    public MessagePublisherTests()
    {
        _mocker = new AutoMocker();
        _channelPoolMock = _mocker.GetMock<IChannelPool>();
        _channelMock = new Mock<IChannel>();

        _channelPoolMock.Setup(x => x.GetAsync())
            .ReturnsAsync(_channelMock.Object);
    }

    [Fact]
    public async Task PublishAsync_WithGenericMessage_ShouldSerializeAndPublish()
    {
        // Arrange
        var message = new TestMessage { Id = 1, Name = "Test" };
        var exchange = "test-exchange";
        var routingKey = "test-routing-key";
        var headers = new Dictionary<string, string> { { "custom-header", "custom-value" } };

        var publisher = _mocker.CreateInstance<MessagePublisher>();

        // Act
        await publisher.PublishAsync(message, exchange, routingKey, headers);

        // Assert
        _channelMock.Verify(x => x.BasicPublishAsync(
            exchange,
            routingKey,
            false,
            It.Is<BasicProperties>(p => p.Persistent && p.Headers != null && p.Headers.ContainsKey("custom-header")),
            It.IsAny<ReadOnlyMemory<byte>>(),
            default), Times.Once);

        _channelPoolMock.Verify(x => x.GetAsync(), Times.Once);
        _channelPoolMock.Verify(x => x.ReturnAsync(_channelMock.Object), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithGenericMessageAndNullHeaders_ShouldPublishWithoutHeaders()
    {
        // Arrange
        var message = new TestMessage { Id = 2, Name = "Test2" };
        var exchange = "test-exchange";
        var routingKey = "test-routing-key";

        var publisher = _mocker.CreateInstance<MessagePublisher>();

        // Act
        await publisher.PublishAsync(message, exchange, routingKey, null);

        // Assert
        _channelMock.Verify(x => x.BasicPublishAsync(
            exchange,
            routingKey,
            false,
            It.Is<BasicProperties>(p => p.Persistent && p.MessageId != null),
            It.IsAny<ReadOnlyMemory<byte>>(),
            default), Times.Once);

        _channelPoolMock.Verify(x => x.ReturnAsync(_channelMock.Object), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithRawBody_ShouldPublishDirectly()
    {
        // Arrange
        var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("raw message"));
        var exchange = "test-exchange";
        var routingKey = "test-routing-key";
        var properties = new BasicProperties { MessageId = "custom-id" };

        var publisher = _mocker.CreateInstance<MessagePublisher>();

        // Act
        await publisher.PublishAsync(body, exchange, routingKey, properties);

        // Assert
        _channelMock.Verify(x => x.BasicPublishAsync(
            exchange,
            routingKey,
            false,
            properties,
            body,
            default), Times.Once);

        _channelPoolMock.Verify(x => x.ReturnAsync(_channelMock.Object), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WhenExceptionOccurs_ShouldReturnChannelAndRethrow()
    {
        // Arrange
        var message = new TestMessage { Id = 3, Name = "Test3" };
        var exchange = "test-exchange";
        var routingKey = "test-routing-key";

        _channelMock.Setup(x => x.BasicPublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<BasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>(),
                default))
            .Throws(new Exception("Publish error"));

        var publisher = _mocker.CreateInstance<MessagePublisher>();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => publisher.PublishAsync(message, exchange, routingKey));

        _channelPoolMock.Verify(x => x.GetAsync(), Times.Once);
        _channelPoolMock.Verify(x => x.ReturnAsync(_channelMock.Object), Times.Once);
    }

    public class TestMessage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
