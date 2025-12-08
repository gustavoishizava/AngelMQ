using AngelMQ.Constants;
using AngelMQ.Properties;
using AngelMQ.Queues;
using Moq;
using Moq.AutoMock;
using RabbitMQ.Client;

namespace AngelMQ.UnitTests.Queues;

public class QueueFactoryTests
{
    private readonly AutoMocker _mocker;
    private readonly Mock<IChannel> _channelMock;
    private readonly QueueProperties<TestMessage> _queueProperties;

    public QueueFactoryTests()
    {
        _mocker = new AutoMocker();
        _channelMock = new Mock<IChannel>();
        _queueProperties = new QueueProperties<TestMessage>
        {
            QueueName = "test-queue",
            Exchange = { Name = "test-exchange", Type = "topic" },
            RoutingKeys = ["test.routing.key"]
        };
    }

    [Fact]
    public async Task CreateAsync_WithoutDeadLetterAndParkingLot_ShouldCreateOnlyMainQueue()
    {
        // Arrange
        _queueProperties.DeadLetter.Enabled = false;
        _queueProperties.ParkingLot.Enabled = false;

        var factory = _mocker.CreateInstance<QueueFactory>();

        // Act
        await factory.CreateAsync(_channelMock.Object, _queueProperties);

        // Assert
        _channelMock.Verify(x => x.ExchangeDeclareAsync(
            "test-exchange",
            "topic",
            true,
            false,
            null,
            false,
            false,
            default), Times.Once);

        _channelMock.Verify(x => x.QueueDeclareAsync(
            "test-queue",
            true,
            false,
            false,
            It.IsAny<IDictionary<string, object?>>(),
            false,
            false,
            default), Times.Once);

        _channelMock.Verify(x => x.QueueBindAsync(
            "test-queue",
            "test-exchange",
            "test.routing.key",
            null,
            false,
            default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDeadLetterEnabled_ShouldCreateMainAndDeadLetterQueues()
    {
        // Arrange
        _queueProperties.DeadLetter.Enabled = true;
        _queueProperties.ParkingLot.Enabled = false;

        var factory = _mocker.CreateInstance<QueueFactory>();

        // Act
        await factory.CreateAsync(_channelMock.Object, _queueProperties);

        // Assert
        // Main queue
        _channelMock.Verify(x => x.QueueDeclareAsync(
            "test-queue",
            true,
            false,
            false,
            It.Is<IDictionary<string, object?>>(d => d.ContainsKey(MessageHeaders.DeadLetterExchange)),
            false,
            false,
            default), Times.Once);

        // Dead letter queue
        _channelMock.Verify(x => x.QueueDeclareAsync(
            "test-queue.dlq",
            true,
            false,
            false,
            null,
            false,
            false,
            default), Times.Once);

        // Dead letter exchange
        _channelMock.Verify(x => x.ExchangeDeclareAsync(
            "test-exchange.dlx",
            "topic",
            true,
            false,
            null,
            false,
            false,
            default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithParkingLotEnabled_ShouldCreateMainAndParkingLotQueues()
    {
        // Arrange
        _queueProperties.DeadLetter.Enabled = false;
        _queueProperties.ParkingLot.Enabled = true;

        var factory = _mocker.CreateInstance<QueueFactory>();

        // Act
        await factory.CreateAsync(_channelMock.Object, _queueProperties);

        // Assert
        // Parking lot queue
        _channelMock.Verify(x => x.QueueDeclareAsync(
            "test-queue.plq",
            true,
            false,
            false,
            It.Is<IDictionary<string, object?>>(d =>
                d.ContainsKey(MessageHeaders.MessageTTL) &&
                d.ContainsKey(MessageHeaders.DeadLetterExchange)),
            false,
            false,
            default), Times.Once);

        // Parking lot exchange
        _channelMock.Verify(x => x.ExchangeDeclareAsync(
            "test-exchange.plx",
            "topic",
            true,
            false,
            null,
            false,
            false,
            default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithAllEnabled_ShouldCreateAllQueues()
    {
        // Arrange
        _queueProperties.DeadLetter.Enabled = true;
        _queueProperties.ParkingLot.Enabled = true;

        var factory = _mocker.CreateInstance<QueueFactory>();

        // Act
        await factory.CreateAsync(_channelMock.Object, _queueProperties);

        // Assert
        // Should create 3 queues: main, dead letter, and parking lot
        _channelMock.Verify(x => x.QueueDeclareAsync(
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<IDictionary<string, object?>>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            default), Times.Exactly(3));

        // Should create 3 exchanges: main, dead letter, and parking lot
        _channelMock.Verify(x => x.ExchangeDeclareAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<IDictionary<string, object?>>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            default), Times.Exactly(3));
    }

    [Fact]
    public async Task CreateAsync_WithAutoCreateFalse_ShouldNotCreateMainExchange()
    {
        // Arrange
        _queueProperties.Exchange.AutoCreate = false;
        _queueProperties.DeadLetter.Enabled = false;
        _queueProperties.ParkingLot.Enabled = false;

        var factory = _mocker.CreateInstance<QueueFactory>();

        // Act
        await factory.CreateAsync(_channelMock.Object, _queueProperties);

        // Assert
        _channelMock.Verify(x => x.ExchangeDeclareAsync(
            "test-exchange",
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<IDictionary<string, object?>>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            default), Times.Never);

        _channelMock.Verify(x => x.QueueDeclareAsync(
            "test-queue",
            true,
            false,
            false,
            It.IsAny<IDictionary<string, object?>>(),
            false,
            false,
            default), Times.Once);

        _channelMock.Verify(x => x.QueueBindAsync(
            "test-queue",
            "test-exchange",
            "test.routing.key",
            null,
            false,
            default), Times.Once);
    }

    public class TestMessage
    {
        public int Id { get; set; }
    }
}
