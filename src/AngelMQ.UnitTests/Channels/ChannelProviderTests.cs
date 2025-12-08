using AngelMQ.Channels;
using AngelMQ.Connections;
using Moq;
using Moq.AutoMock;
using RabbitMQ.Client;

namespace AngelMQ.UnitTests.Channels;

public class ChannelProviderTests
{
    private readonly AutoMocker _mocker;

    public ChannelProviderTests()
    {
        _mocker = new AutoMocker();
    }

    [Fact]
    public async Task GetAsync_WhenCalledFirstTime_ShouldCreateNewChannel()
    {
        // Arrange
        var connectionMock = new Mock<IConnection>();
        var channelMock = new Mock<IChannel>();

        _mocker.GetMock<IConnectionProvider>()
            .Setup(x => x.GetConnectionAsync())
            .ReturnsAsync(connectionMock.Object);

        connectionMock.Setup(x => x.CreateChannelAsync(null,
                                                       It.IsAny<CancellationToken>()))
            .ReturnsAsync(channelMock.Object);

        var provider = _mocker.CreateInstance<ChannelProvider>();

        // Act
        var result = await provider.GetAsync(10);

        // Assert
        Assert.Equal(channelMock.Object, result);
        _mocker.GetMock<IConnectionProvider>().Verify(x => x.GetConnectionAsync(), Times.Once);
        connectionMock.Verify(x => x.CreateChannelAsync(null,
                                                        It.IsAny<CancellationToken>()), Times.Once);
        channelMock.Verify(x => x.BasicQosAsync(0,
                                                10,
                                                false,
                                                default), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WhenCalledMultipleTimes_ShouldReturnSameChannel()
    {
        // Arrange
        var connectionMock = new Mock<IConnection>();
        var channelMock = new Mock<IChannel>();

        _mocker.GetMock<IConnectionProvider>()
            .Setup(x => x.GetConnectionAsync())
            .ReturnsAsync(connectionMock.Object);

        connectionMock.Setup(x => x.CreateChannelAsync(null,
                                                       It.IsAny<CancellationToken>()))
            .ReturnsAsync(channelMock.Object);

        var provider = _mocker.CreateInstance<ChannelProvider>();

        // Act
        var result1 = await provider.GetAsync(10);
        var result2 = await provider.GetAsync(20);

        // Assert
        Assert.Equal(channelMock.Object, result1);
        Assert.Equal(channelMock.Object, result2);
        Assert.Same(result1, result2);
        _mocker.GetMock<IConnectionProvider>().Verify(x => x.GetConnectionAsync(), Times.Once);
        connectionMock.Verify(x => x.CreateChannelAsync(null,
                                                        It.IsAny<CancellationToken>()), Times.Once);
        channelMock.Verify(x => x.BasicQosAsync(0,
                                                10,
                                                false,
                                                default), Times.Once);
    }

    [Fact]
    public async Task CloseAsync_WhenChannelExists_ShouldCloseChannel()
    {
        // Arrange
        var connectionMock = new Mock<IConnection>();
        var channelMock = new Mock<IChannel>();

        _mocker.GetMock<IConnectionProvider>()
            .Setup(x => x.GetConnectionAsync())
            .ReturnsAsync(connectionMock.Object);

        connectionMock.Setup(x => x.CreateChannelAsync(null,
                                                       It.IsAny<CancellationToken>()))
            .ReturnsAsync(channelMock.Object);

        var provider = _mocker.CreateInstance<ChannelProvider>();
        await provider.GetAsync();

        // Act
        await provider.CloseAsync();

        // Assert
        channelMock.Verify(x => x.CloseAsync(It.IsAny<ushort>(),
                                             It.IsAny<string>(),
                                             It.IsAny<bool>(),
                                             It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CloseAsync_WhenChannelDoesNotExist_ShouldNotThrow()
    {
        // Arrange
        var provider = _mocker.CreateInstance<ChannelProvider>();

        // Act & Assert
        await provider.CloseAsync();
    }
}
