using System.Text;
using AngelMQ.Constants;
using AngelMQ.Messages.Errors;
using AngelMQ.Properties;
using AngelMQ.Publishers;
using Moq;
using Moq.AutoMock;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AngelMQ.UnitTests.Messages.Errors;

public class MessageErrorHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly Mock<IChannel> _channelMock;
    private readonly QueueProperties<TestMessage> _queueProperties;

    public MessageErrorHandlerTests()
    {
        _mocker = new AutoMocker();
        _channelMock = new Mock<IChannel>();
        _queueProperties = new QueueProperties<TestMessage>
        {
            QueueName = "test-queue",
            ExchangeName = "test-exchange"
        };
        _queueProperties.ParkingLot.Enabled = true;
        _queueProperties.ParkingLot.SetMaxRetryAttempts(3);
    }

    [Fact]
    public async Task HandleAsync_WhenMaxRetriesNotReached_ShouldSendToParkingLot()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("{\"id\":1}"));
        var properties = new BasicProperties
        {
            Headers = new Dictionary<string, object?>
            {
                [MessageHeaders.Retries] = 1
            }
        };

        var deliveryArgs = new BasicDeliverEventArgs(
            consumerTag: "consumer-tag",
            deliveryTag: 123,
            redelivered: false,
            exchange: "test-exchange",
            routingKey: "test-routing-key",
            properties: properties,
            body: body,
            cancellationToken: default);

        var handler = _mocker.CreateInstance<MessageErrorHandler>();

        // Act
        await handler.HandleAsync(_queueProperties, _channelMock.Object, deliveryArgs, exception);

        // Assert
        _channelMock.Verify(x => x.BasicAckAsync(123, false, default), Times.Once);
        _mocker.GetMock<IMessagePublisher>().Verify(
            x => x.PublishAsync(
                body,
                _queueProperties.ParkingLotExchangeName,
                "test-routing-key",
                It.Is<BasicProperties>(p =>
                    p.Headers != null &&
                    p.Headers.ContainsKey(MessageHeaders.Retries) &&
                    (int)p.Headers[MessageHeaders.Retries]! == 2)),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenMaxRetriesReached_ShouldSendToDeadLetter()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("{\"id\":1}"));
        var properties = new BasicProperties
        {
            Headers = new Dictionary<string, object?>
            {
                [MessageHeaders.Retries] = 3
            }
        };

        var deliveryArgs = new BasicDeliverEventArgs(
            consumerTag: "consumer-tag",
            deliveryTag: 123,
            redelivered: false,
            exchange: "test-exchange",
            routingKey: "test-routing-key",
            properties: properties,
            body: body,
            cancellationToken: default);

        var handler = _mocker.CreateInstance<MessageErrorHandler>();

        // Act
        await handler.HandleAsync(_queueProperties, _channelMock.Object, deliveryArgs, exception);

        // Assert
        _channelMock.Verify(x => x.BasicRejectAsync(123, false, default), Times.Once);
        _mocker.GetMock<IMessagePublisher>().Verify(
            x => x.PublishAsync(
                It.IsAny<ReadOnlyMemory<byte>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<BasicProperties>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenParkingLotDisabled_ShouldSendToDeadLetter()
    {
        // Arrange
        _queueProperties.ParkingLot.Enabled = false;

        var exception = new Exception("Test exception");
        var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("{\"id\":1}"));
        var properties = new BasicProperties
        {
            Headers = new Dictionary<string, object?>()
        };

        var deliveryArgs = new BasicDeliverEventArgs(
            consumerTag: "consumer-tag",
            deliveryTag: 123,
            redelivered: false,
            exchange: "test-exchange",
            routingKey: "test-routing-key",
            properties: properties,
            body: body,
            cancellationToken: default);

        var handler = _mocker.CreateInstance<MessageErrorHandler>();

        // Act
        await handler.HandleAsync(_queueProperties, _channelMock.Object, deliveryArgs, exception);

        // Assert
        _channelMock.Verify(x => x.BasicRejectAsync(123, false, default), Times.Once);
        _mocker.GetMock<IMessagePublisher>().Verify(
            x => x.PublishAsync(
                It.IsAny<ReadOnlyMemory<byte>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<BasicProperties>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenNoRetryHeader_ShouldStartWithRetryCountOne()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("{\"id\":1}"));
        var properties = new BasicProperties
        {
            Headers = new Dictionary<string, object?>()
        };

        var deliveryArgs = new BasicDeliverEventArgs(
            consumerTag: "consumer-tag",
            deliveryTag: 123,
            redelivered: false,
            exchange: "test-exchange",
            routingKey: "test-routing-key",
            properties: properties,
            body: body,
            cancellationToken: default);

        var handler = _mocker.CreateInstance<MessageErrorHandler>();

        // Act
        await handler.HandleAsync(_queueProperties, _channelMock.Object, deliveryArgs, exception);

        // Assert
        _mocker.GetMock<IMessagePublisher>().Verify(
            x => x.PublishAsync(
                body,
                _queueProperties.ParkingLotExchangeName,
                "test-routing-key",
                It.Is<BasicProperties>(p =>
                    p.Headers != null &&
                    p.Headers.ContainsKey(MessageHeaders.Retries) &&
                    (int)p.Headers[MessageHeaders.Retries]! == 1)),
            Times.Once);
    }

    private class TestMessage
    {
        public int Id { get; set; }
    }
}
