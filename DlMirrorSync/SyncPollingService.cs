namespace DlMirrorSync;

public sealed class SyncPollingService : BackgroundService
{
    private readonly SyncService _syncService;
    private readonly ILogger<SyncPollingService> _logger;
    private readonly IConfiguration _configuration;

    public SyncPollingService(
        SyncService syncService,
        ILogger<SyncPollingService> logger,
        IConfiguration configuration) =>
        (_syncService, _logger, _configuration) = (syncService, logger, configuration);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // default to once a day
                var delay = _configuration.GetValue<int>("DlMirrorSync:PollingIntervalMinutes", 1440);

                await _syncService.SyncSubscriptions(stoppingToken);

                _logger.LogInformation("Waiting {delay} minutes", delay);
                await Task.Delay(TimeSpan.FromMinutes(delay), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // When the stopping token is canceled, for example, a call made from services.msc,
            // we shouldn't exit with a non-zero exit code. In other words, this is expected...
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);

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
}