namespace App;

using App.Handlers;
using App.Models;
using System.Text.Json;
using NATS.Client.Core;


public sealed class UpdateService
{
    private NatsConnection _nats;
    private INatsSub<string> _subscription;

    private readonly EventHandler _eventHandler;
    private readonly ILogger<UpdateService> _logger;
    private readonly IConfiguration _configuration;


    public UpdateService(ILogger<UpdateService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _eventHandler = new EventHandler();
    }

    public void Start()
    {
        Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        if (_nats == null)
                        {
                            await Connect();
                        }

                        if (_subscription == null)
                        {
                            await Subscribe();
                        }

                        await Task.Delay(TimeSpan.FromMinutes(1));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{Message}", ex.Message);

                    // Retry after 15 seconds
                    await Task.Delay(TimeSpan.FromSeconds(15));
                }
            }
        );
    }

    public async Task Connect()
    {
        if (_nats != null)
        {
            return;
        }

        var url = _configuration["NATS_URL"];
        _nats = new NatsConnection(new NatsOpts
            {
                Url = url
            }
        );

        Console.WriteLine($"Connecting to NATS Server at {url}");

        // Connections are lazy, so we need to connect explicitly
        // to avoid any races between subscription and publishing.
        await _nats.ConnectAsync();
    }

    public async Task Subscribe()
    {
        if (_nats == null)
        {
            _logger.LogError("NATS connection is not initialized.");
            return;
        }

        _subscription = await _nats.SubscribeCoreAsync<string>("2020610.update.patch.*");
        Console.WriteLine("Subscribed to update.patch.*");

        await foreach (var msg in _subscription.Msgs.ReadAllAsync())
        {
            Console.WriteLine($"Received {msg.Subject}: {msg.Data}\n");

            try
            {
                var patch = JsonSerializer.Deserialize<Patch>(msg.Data);
                _eventHandler.HandlePatch(patch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
            }
        }
    }

    public async Task Unsubscribe()
    {
        if (_nats == null || _subscription == null)
        {
            _logger.LogError("NATS connection is not initialized.");
            return;
        }

        await _subscription.UnsubscribeAsync();
        Console.WriteLine("Unsubscribed from service.update.*");
    }

}
