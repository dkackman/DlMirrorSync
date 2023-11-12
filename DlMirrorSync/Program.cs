using DlMirrorSync;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Data Layer Mirror Sync Service";
});

if (OperatingSystem.IsWindows())
{
    LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(builder.Services);
}

builder.Services.AddSingleton(services =>
    {
        return new ChiaService(
            args.FirstOrDefault(),
            services.GetRequiredService<ILogger<ChiaService>>(),
            services.GetRequiredService<IConfiguration>());
    });
builder.Services.AddSingleton<MirrorService>();
builder.Services.AddSingleton<SyncService>();
builder.Services.AddHostedService<DlMirrorSyncService>();

IHost host = builder.Build();
host.Run();