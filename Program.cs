using App;

using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using Serilog;
using Serilog.Events;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "PatchHub Service";
});

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    LoggerProviderOptions
        .RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(builder.Services);
}

builder.Services.AddSingleton<UpdateService>();
builder.Services.AddHostedService<WindowsBackgroundService>();

builder.Services.AddLogging(configure => {
    var ConsoleLogLevel = builder.Configuration.GetSection("Serilog:ConsoleLogLevel").Get<LogEventLevel>();
    var FileLogLevel = builder.Configuration.GetSection("Serilog:FileLogLevel").Get<LogEventLevel>();

    var logger = new LoggerConfiguration()
    .WriteTo.Console(ConsoleLogLevel, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {CorrelationId} {Level:u3}] {Username} {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("Logs/log.txt", FileLogLevel, "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {CorrelationId} {Level:u3}] {Username} {Message:lj}{NewLine}{Exception}", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 365)
    .CreateLogger();

    configure.AddSerilog(logger);
});
IHost host = builder.Build();
host.Run(); 