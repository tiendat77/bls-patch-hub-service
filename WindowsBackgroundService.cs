namespace App;

public sealed class WindowsBackgroundService : BackgroundService, IDisposable
{
    private readonly UpdateService updateService;
    private readonly ILogger<WindowsBackgroundService> logger;

    public WindowsBackgroundService(UpdateService updateService, ILogger<WindowsBackgroundService> logger)
    {
        this.updateService = updateService;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await updateService.StartAsync();
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // When the stopping token is canceled, for example, a call made from services.msc,
            // we shouldn't exit with a non-zero exit code. In other words, this is expected...
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Message}", ex.Message);
            await updateService.Unsubscribe();

            // Terminates this process and returns an exit code to the operating system.
            // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
            // performs one of two scenarios:
            // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
            // 2. When set to "StopHost": will cleanly stop the host, and log errors.
            //
            // In order for the Windows Service Management system to leverage configured
            // recovery options, we need to terminate the process with a non-zero exit code.
            Environment.Exit(1);
        }
    }

    public override async void Dispose()
    {
        await updateService.Unsubscribe();
    }


}