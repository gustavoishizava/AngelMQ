using System.Security.Authentication;
using AngelMQ.Consumer.Listeners.Handlers;
using AngelMQ.Consumer.Listeners.Messages;
using AngelMQ.Consumer.Workers;
using AngelMQ.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRabbitMQ(options =>
{
    options.ConsumerDispatchConcurrency = 10;
    options.ChannelPool.SetMaxSize(5);
    options.ChannelPool.SetTimeout(10000);
});

builder.Services.AddConsumer<SampleMessageHandler, SampleMessage>(queueProps =>
{
    queueProps.QueueName = "accounts";
    queueProps.ExchangeName = "accounts.exchange";
    queueProps.ExchangeType = "topic";
    queueProps.RoutingKeys = ["create.#", "update.#"];
    queueProps.EnableDeadLetter = true;
    queueProps.EnableParkingLot = true;
    queueProps.ConsumerCount = 2;
    queueProps.PrefetchCount = 250;
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
