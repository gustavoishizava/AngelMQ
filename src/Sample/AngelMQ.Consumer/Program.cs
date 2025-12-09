using AngelMQ.Consumer.Listeners.Handlers;
using AngelMQ.Consumer.Listeners.Messages;
using AngelMQ.Consumer.Publishers;
using AngelMQ.Consumer.QueueProviders;
using AngelMQ.Consumer.Workers;
using AngelMQ.Extensions;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRabbitMQ(options =>
{
    options.ConnectionFactory.ConsumerDispatchConcurrency = 50;
    options.ConnectionFactory.ClientProvidedName = "AngelMQ.Consumer.SampleApp";
    options.ChannelPool.SetMaxSize(5);
    options.ChannelPool.SetTimeout(10000);
});

builder.Services.AddConsumer<SampleQueueProvider, SampleMessageHandler, SampleMessage>();
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
builder.Services.AddConsumer<QueueHandler, QueueMessage>(queueProps =>
{
    queueProps.QueueName = "notifications";
    queueProps.ConsumerCount = 1;
    queueProps.PrefetchCount = 100;
});

builder.Services.AddExchangePublisher<SampleMessage, SampleExchangePublisher>(props =>
{
    props.Configuration.Name = "accounts.exchange";
    props.Configuration.Type = "topic";
    props.AutoCreate = true;
}).AddQueuePublisher<QueueMessage, SampleQueuePublisher>(props =>
{
    props.Configuration.Name = "notifications";
    props.AutoCreate = true;
});

builder.Services.AddHostedService<ProducerWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
