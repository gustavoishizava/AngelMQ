using System.Text;
using System.Text.Json;
using AngelMQ.Messages;
using Microsoft.Extensions.Logging;
using Moq.AutoMock;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AngelMQ.UnitTests.Messages;

public class MessageHandlerTests
{
    private readonly AutoMocker _mocker;
    public MessageHandlerTests()
    {
        _mocker = new AutoMocker();
    }

    [Fact]
    public async Task HandleAsync_ShouldDeserializeMessageAndCallProcessAsync()
    {
        // Arrange
        var testMessage = new TestMessage { Id = 1, Name = "Test" };
        var json = JsonSerializer.Serialize(testMessage, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(json));
        var properties = new BasicProperties
        {
            Headers = new Dictionary<string, object?>
            {
                ["custom-header"] = Encoding.UTF8.GetBytes("custom-value")
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

        var handler = _mocker.CreateInstance<TestMessageHandler>();

        // Act
        await handler.HandleAsync(deliveryArgs);

        // Assert
        Assert.NotNull(handler.ProcessedMessage);
        Assert.Equal(1, handler.ProcessedMessage.Id);
        Assert.Equal("Test", handler.ProcessedMessage.Name);
        Assert.NotNull(handler.ProcessedHeaders);
        Assert.True(handler.ProcessedHeaders.ContainsKey("custom-header"));
        Assert.Equal("custom-value", handler.ProcessedHeaders["custom-header"]);
    }

    [Fact]
    public async Task HandleAsync_WithNullHeaders_ShouldPassEmptyDictionary()
    {
        // Arrange
        var testMessage = new TestMessage { Id = 2, Name = "Test2" };
        var json = JsonSerializer.Serialize(testMessage, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(json));
        var properties = new BasicProperties();

        var deliveryArgs = new BasicDeliverEventArgs(
            consumerTag: "consumer-tag",
            deliveryTag: 123,
            redelivered: false,
            exchange: "test-exchange",
            routingKey: "test-routing-key",
            properties: properties,
            body: body,
            cancellationToken: default);

        var handler = _mocker.CreateInstance<TestMessageHandler>();

        // Act
        await handler.HandleAsync(deliveryArgs);

        // Assert
        Assert.NotNull(handler.ProcessedHeaders);
        Assert.Empty(handler.ProcessedHeaders);
    }

    [Fact]
    public async Task HandleAsync_WithMultipleHeaderTypes_ShouldConvertToString()
    {
        // Arrange
        var testMessage = new TestMessage { Id = 3, Name = "Test3" };
        var json = JsonSerializer.Serialize(testMessage, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(json));
        var properties = new BasicProperties
        {
            Headers = new Dictionary<string, object?>
            {
                ["byte-array-header"] = Encoding.UTF8.GetBytes("byte-value"),
                ["int-header"] = 42,
                ["string-header"] = "string-value"
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

        var handler = _mocker.CreateInstance<TestMessageHandler>();

        // Act
        await handler.HandleAsync(deliveryArgs);

        // Assert
        Assert.NotNull(handler.ProcessedHeaders);
        Assert.Equal("byte-value", handler.ProcessedHeaders["byte-array-header"]);
        Assert.Equal("42", handler.ProcessedHeaders["int-header"]);
        Assert.Equal("string-value", handler.ProcessedHeaders["string-header"]);
    }

    public class TestMessage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TestMessageHandler(ILogger<TestMessageHandler> logger) : MessageHandler<TestMessage>(logger)
    {
        public TestMessage? ProcessedMessage { get; private set; }
        public IDictionary<string, string>? ProcessedHeaders { get; private set; }
        public BasicDeliverEventArgs? ProcessedArgs { get; private set; }

        protected override Task ProcessAsync(TestMessage? message, IDictionary<string, string> headers, BasicDeliverEventArgs args)
        {
            ProcessedMessage = message;
            ProcessedHeaders = headers;
            ProcessedArgs = args;
            return Task.CompletedTask;
        }
    }
}
