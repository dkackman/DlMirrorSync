namespace DlMirrorSync;

using chia.dotnet;

public sealed class SyncService
{
    private readonly DataLayerProxy _dataLayer;
    private readonly ChiaService _chiaService;
    private readonly MirrorService _mirrorService;
    private readonly ILogger<SyncService> _logger;
    private readonly IConfiguration _configuration;

    public SyncService(DataLayerProxy dataLayer,
                        ChiaService chiaService,
                        MirrorService mirrorService,
                        ILogger<SyncService> logger,
                        IConfiguration configuration) =>
            (_dataLayer, _chiaService, _mirrorService, _logger, _configuration) = (dataLayer, chiaService, mirrorService, logger, configuration);

    public async Task SyncSubscriptions(CancellationToken stoppingToken)
    {
        var xchWallet = _chiaService.GetWallet(_configuration.GetValue<uint>("DlMirrorSync:XchWalletId", 1));

        _logger.LogInformation("Syncing mirrors...");
        try
        {
            var addMirrorAmount = _configuration.GetValue<ulong>("DlMirrorSync:AddMirrorAmount", 300000000);
            var fee = await _chiaService.GetFee(addMirrorAmount, stoppingToken);

            _logger.LogInformation("Fetching subscriptions...");
            var subscriptions = await _dataLayer.Subscriptions(stoppingToken);

            await foreach (var id in _mirrorService.FetchLatest(stoppingToken))
            {
                if (!subscriptions.Contains(id))
                {
                    _logger.LogInformation("Subscribing to {id}", id);
                    await _dataLayer.Subscribe(id, Enumerable.Empty<string>(), stoppingToken);
                }

                var mirrors = await _dataLayer.GetMirrors(id, stoppingToken);
                // add any mirrors that aren't already ours
                if (!mirrors.Any(m => m.Ours))
                {
                    var balance = await xchWallet.GetBalance(stoppingToken);
                    if (addMirrorAmount + fee < balance.SpendableBalance)
                    {
                        _logger.LogInformation("Adding mirror {id}", id);
                        await _dataLayer.AddMirror(id, addMirrorAmount, Enumerable.Empty<string>(), fee, stoppingToken);
                    }
                    else if (balance.SpendableBalance == 0 && (addMirrorAmount + fee < balance.PendingChange || addMirrorAmount + fee < balance.ConfirmedWalletBalance))
                    {
                        // no more spendable funds but we have change incoming, pause and see if it has arrived
                        var waitingForChangeDelayMinutes = _configuration.GetValue("App:WaitingForChangeDelayMinutes", 2);
                        _logger.LogWarning("Waiting {WaitingForChangeDelayMinutes} minutes for change", waitingForChangeDelayMinutes);
                        await Task.Delay(TimeSpan.FromMinutes(waitingForChangeDelayMinutes), stoppingToken);
                    }
                    else
                    {
                        _logger.LogWarning("Insufficient funds to add mirror {id}. Balance={SpendableBalance}, Cost={addMirrorAmount}, Fee={fee}", id, balance.SpendableBalance, addMirrorAmount, fee);
                        _logger.LogWarning("Pausing sync for now");

                        // stop trying to add mirrors until we have more funds
                        return;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
        }
    }
}
