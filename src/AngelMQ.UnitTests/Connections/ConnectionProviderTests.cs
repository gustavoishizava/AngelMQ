using AngelMQ.Connections;
using AngelMQ.Properties;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace AngelMQ.UnitTests.Connections;

public class ConnectionProviderTests
{
    private readonly AutoMocker _mocker;
    private readonly ConnectionProperties _connectionProperties;

    public ConnectionProviderTests()
    {
        _mocker = new AutoMocker();
        _connectionProperties = new ConnectionProperties
        {
            HostName = "localhost",
            MaxRetryAttempts = 3,
            DelayMultiplier = 1
        };

        _mocker.Use(Options.Create(_connectionProperties));
    }

    [Fact]
    public async Task GetConnectionAsync_WhenCalledFirstTime_ShouldCreateNewConnection()
    {
        // Arrange
        var connectionMock = new Mock<IConnection>();
        connectionMock.Setup(x => x.IsOpen).Returns(true);

        _mocker.GetMock<IConnectionFactory>()
            .Setup(x => x.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(connectionMock.Object);

        var provider = _mocker.CreateInstance<ConnectionProvider>();

        // Act
        var result = await provider.GetConnectionAsync();

        // Assert
        Assert.Equal(connectionMock.Object, result);
        _mocker.GetMock<IConnectionFactory>()
            .Verify(x => x.CreateConnectionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetConnectionAsync_WhenCalledMultipleTimes_ShouldReturnSameConnection()
    {
        // Arrange
        var connectionMock = new Mock<IConnection>();
        connectionMock.Setup(x => x.IsOpen).Returns(true);

        _mocker.GetMock<IConnectionFactory>()
            .Setup(x => x.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(connectionMock.Object);

        var provider = _mocker.CreateInstance<ConnectionProvider>();

        // Act
        var result1 = await provider.GetConnectionAsync();
        var result2 = await provider.GetConnectionAsync();

        // Assert
        Assert.Equal(connectionMock.Object, result1);
        Assert.Equal(connectionMock.Object, result2);
        Assert.Same(result1, result2);
        _mocker.GetMock<IConnectionFactory>()
            .Verify(x => x.CreateConnectionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetConnectionAsync_WhenConnectionFails_ShouldRetryAndConnectSuccessfully()
    {
        // Arrange
        var connectionMock = new Mock<IConnection>();
        connectionMock.Setup(x => x.IsOpen).Returns(true);

        _mocker.GetMock<IConnectionFactory>()
            .SetupSequence(x => x.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConnectFailureException("Connection failed", new Exception("Inner exception")))
            .ThrowsAsync(new BrokerUnreachableException(new Exception("Connection failed")))
            .ReturnsAsync(connectionMock.Object);

        var provider = _mocker.CreateInstance<ConnectionProvider>();

        // Act
        var result = await provider.GetConnectionAsync();

        // Assert
        Assert.Equal(connectionMock.Object, result);
        _mocker.GetMock<IConnectionFactory>()
            .Verify(x => x.CreateConnectionAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task GetConnectionAsync_WhenConnectionIsClosedAndReconnects_ShouldCreateNewConnection()
    {
        // Arrange
        var firstConnectionMock = new Mock<IConnection>();
        var secondConnectionMock = new Mock<IConnection>();

        firstConnectionMock.Setup(x => x.IsOpen)
            .Returns(false);

        secondConnectionMock.Setup(x => x.IsOpen).Returns(true);

        _mocker.GetMock<IConnectionFactory>()
            .SetupSequence(x => x.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(firstConnectionMock.Object)
            .ReturnsAsync(secondConnectionMock.Object);

        var provider = _mocker.CreateInstance<ConnectionProvider>();

        // Act
        var result1 = await provider.GetConnectionAsync();
        var result2 = await provider.GetConnectionAsync();

        // Assert
        Assert.Equal(firstConnectionMock.Object, result1);
        Assert.Equal(secondConnectionMock.Object, result2);
        _mocker.GetMock<IConnectionFactory>()
            .Verify(x => x.CreateConnectionAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
