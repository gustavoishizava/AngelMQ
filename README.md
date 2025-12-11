# AngelMQ

>A high-performance, ergonomic RabbitMQ wrapper for .NET 8 — focused on making publishers and consumers easy to implement while retaining low-latency and throughput.

## Overview

AngelMQ is a lightweight library that builds on top of the official `RabbitMQ.Client` for .NET 8. It provides opinionated abstractions and helpers for connections, channel pooling, publishers and consumers so you can focus on business logic instead of RabbitMQ plumbing.

Key goals:
- **Performance:** efficient channel pooling and a minimal overhead API surface.
- **Ease of use:** simple, testable patterns for publishers and consumers.
- **.NET 8 friendly:** takes advantage of modern .NET runtime features and async/await patterns.

## Features

- **Connection management:** one connection for publishers and one connection for consumers (managed via `IConnectionProvider`).
- **Channels:** one channel per consumer and a channel pool for publishers (`IChannelPool`, `IChannelProvider`) to minimize channel creation overhead.
- **Simple configuration:** easy, declarative registration and configuration for creating consumers and publishers via DI extension methods.

- **Dead-letter & parking-lot support:** built-in patterns for dead-letter queues (DLQ) and parking-lot queues to capture failed messages for later inspection or reprocessing.

## Getting started

Requirements:
- .NET SDK 8.0
- RabbitMQ server (for runtime)

Install via NuGet

Install the library from NuGet into your project (recommended):

```bash
dotnet add package AngelMQ --version 1.0.0
```

Or add a `PackageReference` to your project file:

```xml
<PackageReference Include="AngelMQ" Version="1.0.0" />
```

Use from your project

- Add a project reference to the `AngelMQ` project, or build a NuGet package if you prefer to publish and consume it that way.

## Quick usage (examples)

The examples below show common patterns and the library's DI extensions. Use these as a starting point for integrating AngelMQ into your application's dependency injection and configuration approach.

1) Configure connection and channel pool

Use the `AddRabbitMQ` extension to configure connection factory options and channel pool settings during service registration. Example:

```csharp
builder.Services.AddRabbitMQ(options =>
{
    options.ConnectionFactory.HostName = "localhost";
    options.ConnectionFactory.UserName = "guest";
    options.ConnectionFactory.Password = "guest";
    options.ConnectionFactory.Port = 5672;
    options.ConnectionFactory.VirtualHost = "/";
    options.ConnectionFactory.ConsumerDispatchConcurrency = 50;
    options.ConnectionFactory.ClientProvidedName = "AngelMQ.Consumer.SampleApp";
    options.ChannelPool.SetMaxSize(5);
    options.ChannelPool.SetTimeout(10000); // milliseconds
});
```

This configures the underlying `ConnectionFactory` (from `RabbitMQ.Client`) and tunes the channel pool used by publishers and background workers.

2) Implement publishers from base classes

Below are concrete examples showing how to create an exchange publisher and a queue publisher by inheriting from the library base classes `ExchangePublisher<T>` and `QueuePublisher<T>`.

Exchange publisher example

```csharp
public sealed class SampleExchangePublisher : ExchangePublisher<SampleMessage>
{
    public SampleExchangePublisher(ILogger<SampleExchangePublisher> logger,
                                   IChannelPool channelPool,
                                   IMessagePublisher messagePublisher,
                                   IOptions<PublisherProperties<SampleMessage, ExchangeProperties>> options)
        : base(logger, channelPool, messagePublisher, options)
    {
    }

    protected override string BuildRoutingKey(SampleMessage message, IDictionary<string, string>? headers = null)
    {
        // Build routing key using message fields
        return $"{message.Country}.{message.Id}";
    }
}
```

Queue publisher example

```csharp
public class SampleQueuePublisher : QueuePublisher<QueueMessage>
{
    public SampleQueuePublisher(ILogger<SampleQueuePublisher> logger,
                                IChannelPool channelPool,
                                IMessagePublisher messagePublisher,
                                IOptions<PublisherProperties<QueueMessage, QueueProperties>> options)
        : base(logger, channelPool, messagePublisher, options)
    {
    }
}
```

Register these concrete publishers in DI using the existing extension methods. Example registration:

```csharp
builder.Services.AddExchangePublisher<SampleMessage, SampleExchangePublisher>(props =>
{
    props.Configuration.Name = "accounts.exchange";
    props.Configuration.Type = "topic";
    props.AutoCreate = true;
});

builder.Services..AddQueuePublisher<QueueMessage, SampleQueuePublisher>(props =>
{
    props.Configuration.Name = "notifications";
    props.AutoCreate = true;
});
```

The typed `IPublisher<T>` implementations (e.g. `SampleExchangePublisher`, `SampleQueuePublisher`) give you a convenient, strongly-typed way to publish domain messages.

3) Publish directly using `IMessagePublisher` or typed `IPublisher<T>`

If you prefer to publish messages directly, you can inject `IMessagePublisher` (generic or non-generic) or the typed `IPublisher<T>` into a service or background worker.

Using the generic typed publisher (`IPublisher<T>`):

```csharp
public class MyService
{
    private readonly IPublisher<SampleMessage> _publisher;

    public MyService(IPublisher<SampleMessage> publisher)
    {
        _publisher = publisher;
    }

    public Task SendSampleAsync()
    {
        return _publisher.PublishAsync(new SampleMessage { Id = 1, Country = "br" });
    }
}
```

Using `IMessagePublisher` directly (routing to an exchange or queue):

```csharp
public class DirectPublisherService
{
    private readonly IMessagePublisher _messagePublisher;

    public DirectPublisherService(IMessagePublisher messagePublisher)
    {
        _messagePublisher = messagePublisher;
    }

    public Task PublishToExchangeAsync()
    {
        var message = new SampleMessage { Id = 42, Country = "mx" };
        // Publish to exchange with a routing key
        return _messagePublisher.PublishAsync(message, "accounts.exchange", "routing.key");
    }

    public Task PublishToQueueAsync()
    {
        var queueMessage = new QueueMessage { Id = 7 };
        // Publish directly to a queue by using empty exchange and queue name as routing key
        return _messagePublisher.PublishAsync(queueMessage, string.Empty, "notifications");
    }
}
```

These direct calls match the library's internal usage (typed publishers call into `IMessagePublisher`). Use whichever approach fits your application's abstraction level.

Consumers — registration examples

AngelMQ provides flexible registration helpers for consumers. Below are three common registration patterns. The queue provider variant is shown last.

1) Full consumer properties (exchange + queue inline)

Register a consumer and pass all consumer/queue/exchange configuration inline via a lambda. Use this when you want to configure a single queue with explicit settings:

```csharp
builder.Services.AddConsumer<SampleMessageHandler, SampleMessage>(queueProps =>
{
    queueProps.QueueName = "accounts";
    queueProps.Exchange = new() { Name = "accounts.exchange", Type = ExchangeType.Topic };
    queueProps.RoutingKeys = ["#"];
    queueProps.DeadLetter.Enabled = true;
    queueProps.ParkingLot.Enabled = true;
    queueProps.ConsumerCount = 1;
    queueProps.PrefetchCount = 250;
});
```

2) Queue only

Register a consumer that listens directly to a queue without specifying an exchange on the registration call:

```csharp
builder.Services.AddConsumer<QueueHandler, QueueMessage>(queueProps =>
{
    queueProps.QueueName = "notifications";
    queueProps.ConsumerCount = 1;
    queueProps.PrefetchCount = 100;
});
```

3) Queue provider (exchange + queue via provider)

Use a `IQueueProvider<TMessage>` implementation to supply multiple queue definitions and properties from code (for example, when you need to programmatically return multiple queues per tenant/region):

```csharp
builder.Services.AddConsumer<SampleQueueProvider, SampleMessageHandler, SampleMessage>();
```

`SampleQueueProvider` returns a list of `QueueProperties<SampleMessage>` — each entry defines `QueueName`, optional `Exchange` settings, `RoutingKeys`, `ConsumerCount`, `PrefetchCount`, dead-letter and parking-lot settings, etc.

Queue provider implementation example

Here's a concrete `IQueueProvider<TMessage>` implementation that returns multiple queue definitions. This mirrors the sample `SampleQueueProvider` in the repository and is useful when you need to register multiple queues programmatically (for example, per region or tenant):

```csharp
using AngelMQ.Consumer.Listeners.Messages;
using AngelMQ.Properties.Consumers;
using System.Collections.Generic;
using System.Threading.Tasks;

public sealed class SampleQueueProvider : IQueueProvider<SampleMessage>
{
    public Task<List<QueueProperties<SampleMessage>>> GetQueuePropertiesAsync()
    {
        var br = new QueueProperties<SampleMessage>
        {
            QueueName = "br.accounts",
            Exchange = new ExchangeProperties
            {
                Name = "accounts.exchange",
                Type = "topic",
                AutoCreate = false
            },
            RoutingKeys = new[] { "br.*" },
            ConsumerCount = 2,
            PrefetchCount = 250
        };
        br.DeadLetter.Enabled = true;
        br.ParkingLot.Enabled = true;

        var mx = new QueueProperties<SampleMessage>
        {
            QueueName = "mx.accounts",
            Exchange = new ExchangeProperties
            {
                Name = "accounts.exchange",
                Type = "topic",
                AutoCreate = false
            },
            RoutingKeys = new[] { "mx.*" },
            ConsumerCount = 1,
            PrefetchCount = 250
        };
        mx.DeadLetter.Enabled = true;
        mx.ParkingLot.Enabled = true;

        return Task.FromResult(new List<QueueProperties<SampleMessage>> { br, mx });
    }
}
```

Register the provider with the DI helper:

```csharp
builder.Services.AddConsumer<SampleQueueProvider, SampleMessageHandler, SampleMessage>();
```

This will cause the library to read the returned `QueueProperties<TMessage>` entries and create workers for each configured queue.

When to use each pattern:
- Use the inline properties overload when you want to declare queue and exchange settings per consumer at registration time.
- Use the queue-only overload for simple queue listeners.
- Use a queue provider when you need multiple queues returned dynamically.

Handler implementation example

Below is a typical `MessageHandler<T>` implementation that processes messages and uses the provided logger. `MessageHandler<T>` handles common ACK/NACK semantics; override `ProcessAsync` to implement business logic.

```csharp
using AngelMQ.Consumer.Listeners.Messages;
using AngelMQ.Messages;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;

public class SampleMessageHandler : MessageHandler<SampleMessage>
{
    private readonly ILogger<MessageHandler<SampleMessage>> _logger;

    public SampleMessageHandler(ILogger<MessageHandler<SampleMessage>> logger)
        : base(logger)
    {
        _logger = logger;
    }

    protected override async Task ProcessAsync(SampleMessage? message, IDictionary<string, string> headers, BasicDeliverEventArgs args)
    {
        _logger.LogInformation("Processing SampleMessage with Id: {Id}", message?.Id);
        // Simulate some work
        await Task.Delay(Random.Shared.Next(20, 100));
        // business logic here
    }
}
```

 


## Contributing

- Create issues for bugs and feature requests.
- Fork the repository, make changes, and open a pull request with tests for new behavior.
- Keep changes focused and add unit tests under `src/AngelMQ.UnitTests`.

## License

This project is licensed under the MIT License — see the `LICENSE` file for details.

Copyright (c) 2025 gustavoishizava

## Questions / Contact

For questions or feedback open an issue in this repository or contact the maintainer.

