using AngelMQ.Channels.Pool;
using AngelMQ.Properties.Publishers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AngelMQ.Publishers.Base;

public abstract class PublisherBase<TMessage>(ILogger<PublisherBase<TMessage>> logger,
                                              IChannelPool channelPool,
                                              IMessagePublisher messagePublisher,
                                              IOptions<PublisherProperties<TMessage>> options)
                                              : IPublisher<TMessage> where TMessage : class
{
    protected readonly ILogger<PublisherBase<TMessage>> logger = logger;
    protected readonly PublisherProperties<TMessage> properties = options.Value;
    protected readonly IMessagePublisher messagePublisher = messagePublisher;

    private bool _created = false;
    private readonly SemaphoreSlim _setupSemaphore = new(1, 1);

    public async Task PublishAsync(TMessage message, IDictionary<string, string>? headers = null)
    {
        if (!_created)
            await TrySetupAsync();

        await SendAsync(message, headers);
    }

    private async Task TrySetupAsync()
    {
        try
        {
            await _setupSemaphore.WaitAsync();

            if (_created)
                return;

            if (!properties.AutoCreate)
            {
                _created = true;
                return;
            }

            await InternalSetupAsync();
        }
        finally
        {
            _setupSemaphore.Release();
        }
    }

    private async Task InternalSetupAsync()
    {
        var channel = await channelPool.GetAsync();

        try
        {
            await SetupAsync(channel);
            _created = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while trying to create exchange or queue.");
            throw;
        }
        finally
        {
            await channelPool.ReturnAsync(channel);
        }
    }

    protected abstract Task SetupAsync(IChannel channel);

    protected abstract Task SendAsync(TMessage message, IDictionary<string, string>? headers = null);
}
