namespace DlMirrorSync;

public sealed class SyncService
{
    private readonly ILogger<SyncService> _logger;
    public SyncService(ILogger<SyncService> logger) => _logger = logger;
    public async Task SyncSubscriptions(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
    }
}