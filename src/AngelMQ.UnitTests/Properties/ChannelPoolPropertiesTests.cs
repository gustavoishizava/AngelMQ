using AngelMQ.Properties;

namespace AngelMQ.UnitTests.Properties;

public class ChannelPoolPropertiesTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void SetMaxSize_WithValidValue_ShouldSetMaxSize(int expectedMaxSize)
    {
        // Arrange
        var properties = new ChannelPoolProperties();

        // Act
        properties.SetMaxSize(expectedMaxSize);

        // Assert
        Assert.Equal(expectedMaxSize, properties.MaxSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void SetMaxSize_WithValueLessThanMinimum_ShouldThrowArgumentOutOfRangeException(int invalidMaxSize)
    {
        // Arrange
        var properties = new ChannelPoolProperties();

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => properties.SetMaxSize(invalidMaxSize));
        Assert.Equal("maxSize", exception.ParamName);
    }

    [Theory]
    [InlineData(System.Threading.Timeout.Infinite)]
    [InlineData(1000)]
    public void SetTimeout_WithValidValue_ShouldSetTimeout(int expectedTimeout)
    {
        // Arrange
        var properties = new ChannelPoolProperties();

        // Act
        properties.SetTimeout(expectedTimeout);

        // Assert
        Assert.Equal(expectedTimeout, properties.Timeout);
    }

    [Fact]
    public void SetTimeout_WithValueLessThanInfinite_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var properties = new ChannelPoolProperties();
        var invalidTimeout = -2;

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => properties.SetTimeout(invalidTimeout));
        Assert.Equal("timeout", exception.ParamName);
    }
}
