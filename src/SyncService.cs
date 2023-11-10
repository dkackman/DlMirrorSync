namespace App.WindowsService;

public sealed class SyncService {
    public async Task Sync(CancellationToken stoppingToken) {
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
    }
}