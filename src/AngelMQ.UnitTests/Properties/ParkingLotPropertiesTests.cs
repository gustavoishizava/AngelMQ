using AngelMQ.Properties;

namespace AngelMQ.UnitTests.Properties;

public class ParkingLotPropertiesTests
{
    [Fact]
    public void BuildQueueName_WithDefaultProperties_ShouldReturnMainQueueNameWithDefaultSuffix()
    {
        // Arrange
        var properties = new ParkingLotProperties();
        var mainQueueName = "orders";

        // Act
        var result = properties.BuildQueueName(mainQueueName);

        // Assert
        Assert.Equal("orders.plq", result);
    }

    [Fact]
    public void BuildQueueName_WithCustomQueueNameAndSuffix_ShouldReturnCustomQueueNameWithCustomSuffix()
    {
        // Arrange
        var properties = new ParkingLotProperties
        {
            QueueName = "custom-queue",
            QueueSuffix = ".custom"
        };
        var mainQueueName = "orders";

        // Act
        var result = properties.BuildQueueName(mainQueueName);

        // Assert
        Assert.Equal("custom-queue.custom", result);
    }

    [Fact]
    public void BuildExchangeName_WithDefaultProperties_ShouldReturnMainExchangeNameWithDefaultSuffix()
    {
        // Arrange
        var properties = new ParkingLotProperties();
        var mainExchangeName = "orders-exchange";

        // Act
        var result = properties.BuildExchangeName(mainExchangeName);

        // Assert
        Assert.Equal("orders-exchange.plx", result);
    }

    [Fact]
    public void BuildExchangeName_WithCustomExchangeNameAndSuffix_ShouldReturnCustomExchangeNameWithCustomSuffix()
    {
        // Arrange
        var properties = new ParkingLotProperties
        {
            ExchangeName = "custom-exchange",
            ExchangeSuffix = ".custom"
        };
        var mainExchangeName = "orders-exchange";

        // Act
        var result = properties.BuildExchangeName(mainExchangeName);

        // Assert
        Assert.Equal("custom-exchange.custom", result);
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(5000)]
    public void SetTTL_WithValidValue_ShouldSetTTL(int expectedTTL)
    {
        // Arrange
        var properties = new ParkingLotProperties();

        // Act
        properties.SetTTL(expectedTTL);

        // Assert
        Assert.Equal(expectedTTL, properties.TTL);
    }

    [Fact]
    public void SetTTL_WithValueLessThanMinimum_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var properties = new ParkingLotProperties();
        var invalidTTL = 500;

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => properties.SetTTL(invalidTTL));
        Assert.Equal("ttl", exception.ParamName);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void SetMaxRetryAttempts_WithValidValue_ShouldSetMaxRetryAttempts(ushort expectedMaxRetryAttempts)
    {
        // Arrange
        var properties = new ParkingLotProperties();

        // Act
        properties.SetMaxRetryAttempts(expectedMaxRetryAttempts);

        // Assert
        Assert.Equal(expectedMaxRetryAttempts, properties.MaxRetryAttempts);
    }

    [Fact]
    public void SetMaxRetryAttempts_WithValueLessThanMinimum_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var properties = new ParkingLotProperties();
        ushort invalidMaxRetryAttempts = 0;

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => properties.SetMaxRetryAttempts(invalidMaxRetryAttempts));
        Assert.Equal("maxRetryAttempts", exception.ParamName);
    }
}
