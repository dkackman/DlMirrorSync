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

            var subscriptions = await _dataLayer.Subscriptions(stoppingToken);

            await foreach (var id in _mirrorService.FetchLatest(stoppingToken))
            {
                if (!subscriptions.Contains(id))
                {
                    _logger.LogInformation("Subscribing to mirror {id}", id);
                    await _dataLayer.Subscribe(id, Enumerable.Empty<string>(), stoppingToken);
                }

                var mirrors = await _dataLayer.GetMirrors(id, stoppingToken);
                // add any mirrors that aren't already ours
                if (!mirrors.Any(m => m.Ours))
                {
                    var balance = await xchWallet.GetBalance(stoppingToken);
                    if (addMirrorAmount + fee < balance.SpendableBalance)
                    {
                        await _dataLayer.AddMirror(id, addMirrorAmount, Enumerable.Empty<string>(), fee, stoppingToken);
                    }
                    else
                    {
                        _logger.LogWarning("Insufficient funds to add mirror {id}. Balance={SpendableBalance}, Cost={addMirrorAmount}, Fee={fee}", id, balance.SpendableBalance, addMirrorAmount, fee);
                        _logger.LogWarning("Stopping mirror sync for now.");

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
