using AngelMQ.Properties;

namespace AngelMQ.UnitTests.Properties;

public class QueuePropertiesTests
{
    [Theory]
    [InlineData("orders", "orders-exchange")]
    [InlineData("accounts", "accounts-exchange")]
    public void Properties_ShouldReturnCorrectNamesWithSuffixes(string queueName, string exchangeName)
    {
        // Arrange
        var properties = new QueueProperties<object>
        {
            QueueName = queueName,
            Exchange = { Name = exchangeName }
        };

        // Act & Assert
        Assert.Equal($"{queueName}.dlq", properties.DeadLetterQueueName);
        Assert.Equal($"{exchangeName}.dlx", properties.DeadLetterExchangeName);
        Assert.Equal($"{queueName}.plq", properties.ParkingLotQueueName);
        Assert.Equal($"{exchangeName}.plx", properties.ParkingLotExchangeName);
    }
}
