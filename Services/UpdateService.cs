namespace App;

using App.Handlers;
using App.Models;

using System.Text.Json;
using NATS.Client.Core;
using Microsoft.Data.SqlClient;


public sealed class UpdateService
{
    private NatsConnection _nats;
    private INatsSub<Patch> _subscription;

    private string _storeID;

    private readonly EventHandler _eventHandler;
    private readonly ILogger<UpdateService> _logger;
    private readonly IConfiguration _configuration;


    public UpdateService(ILogger<UpdateService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _eventHandler = new EventHandler();
    }

    public async Task StartAsync()
    {
        while (true)
        {
            try
            {
                if (_nats == null)
                {
                    await Connect();
                }

                if (string.IsNullOrEmpty(_storeID))
                {
                    _storeID = await GetStoreID();
                    _logger.LogInformation($"StoreID: {_storeID}");
                }

                if (_subscription == null)
                {
                    await Subscribe();
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "{Message}", ex.Message);
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }
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

        _logger.LogInformation($"Connecting to NATS Server at {url}");

        // Connections are lazy, so we need to connect explicitly
        // to avoid any races between subscription and publishing.
        await _nats.ConnectAsync();
        _nats.ConnectionOpened += async (e, args) => {
            _logger.LogInformation($"Connected to server");
        };
        _nats.ConnectionDisconnected += async (e, args) => {
            _logger.LogInformation($"Disconnected from server");
        };
    }

    public async Task Subscribe()
    {
        if (_nats == null)
        {
            _logger.LogError("NATS connection is not initialized.");
            return;
        }

        var serializer = new NatsJsonContextSerializer<Patch>(PatchContext.Default);

        _subscription = await _nats.SubscribeCoreAsync<Patch>(
            $"{_storeID}.commands.>",
            serializer: serializer
        );

        _logger.LogInformation("Subscribed to nats chanel " + _storeID);

        await foreach (var msg in _subscription.Msgs.ReadAllAsync())
        {
            _logger.LogInformation($"Received {msg.Subject}: {msg.Data}\n");
            await HandleMessage(msg);
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
        await _nats.DisposeAsync();
        _logger.LogInformation("Unsubscribed from service.update.*");
    }

    private async Task HandleMessage(NatsMsg<Patch> msg)
    {
        try
        {
            ResponseBase result = null;
            var command = msg.Subject.Replace($"{_storeID}.", "");
            var serializer = new NatsJsonContextSerializer<ResponseBase>(ResponseBaseContext.Default);

            switch (command)
            {
                case "commands.install.patch":
                    result = await _eventHandler.HandlePatch(msg.Data);
                    await msg.ReplyAsync(result, serializer: serializer);
                    break;

                default:
                    _logger.LogWarning("Unknown subject: {Subject}", msg.Subject);
                    await msg.ReplyAsync(new ErrorResponse(false, "Unknown command"), serializer: serializer);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
        }
    }

    private async Task<string> GetStoreID()
    {
        string storeID = "";

        try
        {
            string sqlServerUrl = _configuration["SQL_URL"];

            using (SqlConnection connection = new SqlConnection(sqlServerUrl))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    connection.Open();
                }

                string sql = "SELECT StoreID FROM dbo.Stores";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                storeID = reader.GetString(reader.GetOrdinal("StoreID"));
                            }
                        }
                    }
                }
            }
        }
        catch (SqlException e)
        {
            _logger.LogError("{Message}", e.ToString());
        }

        return storeID;
    }

}
