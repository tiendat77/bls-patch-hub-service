using App;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using Serilog.Events;
using Serilog;

internal class Program
{
    static void Main(string[] args)
    {


        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
       
        Log.Logger = new LoggerConfiguration()
               .WriteTo.File(@"C:/Users/blogic/source/repos/PathService/release/Logs/log.txt", LogEventLevel.Information, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {CorrelationId} {Level:u3}] {Username} {Message:lj}{NewLine}{Exception}", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 365)
               .CreateLogger();


        builder.Logging.Services.AddSerilog(Log.Logger);


        builder.Services.AddWindowsService(options =>
        {
            options.ServiceName = "Demo Report Service 222222";
        });

        LoggerProviderOptions.RegisterProviderOptions<
            EventLogSettings, EventLogLoggerProvider>(builder.Services);

        builder.Services.AddSingleton<UpdateService>();
        builder.Services.AddHostedService<WindowsBackgroundService>();

        IHost host = builder.Build();

        host.Run();
    }
}
