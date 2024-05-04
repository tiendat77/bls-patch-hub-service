using App;

using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;

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

builder.Services.AddSingleton<JokeService>();
builder.Services.AddSingleton<UpdateService>();
builder.Services.AddHostedService<WindowsBackgroundService>();

IHost host = builder.Build();
host.Run();
