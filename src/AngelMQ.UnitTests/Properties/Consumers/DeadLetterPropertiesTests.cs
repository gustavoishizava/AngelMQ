using AngelMQ.Properties.Consumers;

namespace AngelMQ.UnitTests.Properties.Consumers;

public class DeadLetterPropertiesTests
{
    [Fact]
    public void BuildQueueName_WithDefaultProperties_ShouldReturnMainQueueNameWithDefaultSuffix()
    {
        // Arrange
        var properties = new DeadLetterProperties();
        var mainQueueName = "orders";

        // Act
        var result = properties.BuildQueueName(mainQueueName);

        // Assert
        Assert.Equal("orders.dlq", result);
    }

    [Fact]
    public void BuildQueueName_WithCustomQueueNameAndSuffix_ShouldReturnCustomQueueNameWithCustomSuffix()
    {
        // Arrange
        var properties = new DeadLetterProperties
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
        var properties = new DeadLetterProperties();
        var mainExchangeName = "orders-exchange";

        // Act
        var result = properties.BuildExchangeName(mainExchangeName);

        // Assert
        Assert.Equal("orders-exchange.dlx", result);
    }

    [Fact]
    public void BuildExchangeName_WithCustomExchangeNameAndSuffix_ShouldReturnCustomExchangeNameWithCustomSuffix()
    {
        // Arrange
        var properties = new DeadLetterProperties
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
}
