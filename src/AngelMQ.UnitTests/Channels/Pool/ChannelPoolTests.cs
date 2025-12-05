using AngelMQ.Channels.Pool;
using AngelMQ.Connections;
using AngelMQ.Properties;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using RabbitMQ.Client;

namespace AngelMQ.UnitTests.Channels.Pool;

public class ChannelPoolTests
{
    private readonly AutoMocker _mocker;
    private readonly ConnectionProperties _connectionProperties;

    public ChannelPoolTests()
    {
        _mocker = new AutoMocker();
        _connectionProperties = new ConnectionProperties
        {
            HostName = "localhost"
        };
        _connectionProperties.ChannelPool.SetMaxSize(2);

        _mocker.Use(Options.Create(_connectionProperties));
    }

    [Fact]
    public async Task GetAsync_WhenPoolIsEmpty_ShouldCreateNewChannel()
    {
        // Arrange
        var connectionMock = new Mock<IConnection>();
        var channelMock = new Mock<IChannel>();

        _mocker.GetMock<IConnectionProvider>()
            .Setup(x => x.GetConnectionAsync())
            .ReturnsAsync(connectionMock.Object);

        connectionMock.Setup(x => x.CreateChannelAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(channelMock.Object);

        var pool = _mocker.CreateInstance<ChannelPool>();

        // Act
        var result = await pool.GetAsync();

        // Assert
        Assert.Equal(1, pool.CurrentPoolSize);
        Assert.Equal(channelMock.Object, result);
        connectionMock.Verify(x => x.CreateChannelAsync(null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReturnAsync_WithOpenChannel_ShouldReturnChannelToPool()
    {
        // Arrange
        var connectionMock = new Mock<IConnection>();
        var channelMock = new Mock<IChannel>();
        channelMock.Setup(x => x.IsOpen).Returns(true);

        _mocker.GetMock<IConnectionProvider>()
            .Setup(x => x.GetConnectionAsync())
            .ReturnsAsync(connectionMock.Object);

        connectionMock.Setup(x => x.CreateChannelAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(channelMock.Object);

        var pool = _mocker.CreateInstance<ChannelPool>();
        var channel = await pool.GetAsync();

        // Act
        await pool.ReturnAsync(channel);
        var reusedChannel = await pool.GetAsync();

        // Assert
        Assert.Equal(1, pool.CurrentPoolSize);
        Assert.Same(channel, reusedChannel);
        connectionMock.Verify(x => x.CreateChannelAsync(null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WhenChannelIsClosed_ShouldCreateNewChannel()
    {
        // Arrange
        var connectionMock = new Mock<IConnection>();
        var closedChannelMock = new Mock<IChannel>();
        var newChannelMock = new Mock<IChannel>();

        closedChannelMock.Setup(x => x.IsOpen).Returns(false);
        newChannelMock.Setup(x => x.IsOpen).Returns(true);

        _mocker.GetMock<IConnectionProvider>()
            .Setup(x => x.GetConnectionAsync())
            .ReturnsAsync(connectionMock.Object);

        connectionMock.SetupSequence(x => x.CreateChannelAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(closedChannelMock.Object)
            .ReturnsAsync(newChannelMock.Object);

        var pool = _mocker.CreateInstance<ChannelPool>();
        var closedChannel = await pool.GetAsync();
        await pool.ReturnAsync(closedChannel);

        // Act
        var result = await pool.GetAsync();

        // Assert
        Assert.Equal(1, pool.CurrentPoolSize);
        Assert.Equal(newChannelMock.Object, result);
        closedChannelMock.Verify(x => x.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task ReturnAsync_WithClosedChannel_ShouldDisposeChannel()
    {
        // Arrange
        var connectionMock = new Mock<IConnection>();
        var channelMock = new Mock<IChannel>();
        channelMock.Setup(x => x.IsOpen).Returns(false);

        _mocker.GetMock<IConnectionProvider>()
            .Setup(x => x.GetConnectionAsync())
            .ReturnsAsync(connectionMock.Object);

        connectionMock.Setup(x => x.CreateChannelAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(channelMock.Object);

        var pool = _mocker.CreateInstance<ChannelPool>();
        var channel = await pool.GetAsync();

        // Act
        await pool.ReturnAsync(channel);

        // Assert
        Assert.Equal(0, pool.CurrentPoolSize);
        channelMock.Verify(x => x.DisposeAsync(), Times.Once);
    }
}
