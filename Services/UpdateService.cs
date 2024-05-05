namespace App;

using App.Handlers;
using App.Models;

using System.Text.Json;
using NATS.Client.Core;
using Microsoft.Data.SqlClient;


public sealed class UpdateService
{
    private NatsConnection _nats;
    private INatsSub<string> _subscription;

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

                        if (string.IsNullOrEmpty(_storeID))
                        {
                            _storeID = await GetStoreID();
                            Console.WriteLine($"StoreID: {_storeID}");
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

        _subscription = await _nats.SubscribeCoreAsync<string>($"{_storeID}.commands.>");
        Console.WriteLine("Subscribed to nats chanel " + _storeID);

        await foreach (var msg in _subscription.Msgs.ReadAllAsync())
        {
            Console.WriteLine($"Received {msg.Subject}: {msg.Data}\n");
            HandleMessage(msg);
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

    private async Task HandleMessage(NatsMsg<string> msg)
    {
        try
        {
            var result = string.Empty;
            var command = msg.Subject.Replace($"{_storeID}.", "");

            switch (command)
            {
                case "commands.install.patch":
                    var patch = JsonSerializer.Deserialize<Patch>(
                        msg.Data,
                        new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }
                    );
                    result = await _eventHandler.HandlePatch(patch);
                    await msg.ReplyAsync(result);
                    break;

                case "commands.execute.command":
                    result = await _eventHandler.HandleCommand(msg.Data);
                    await msg.ReplyAsync(result);
                    break;

                default:
                    _logger.LogWarning("Unknown subject: {Subject}", msg.Subject);
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
                if (connection.State == System.Data.ConnectionState.Closed) {
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
