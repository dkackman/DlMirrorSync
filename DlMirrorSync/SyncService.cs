namespace DlMirrorSync;

public sealed class SyncService
{
    private readonly ChiaService _chiaService;
    private readonly MirrorService _mirrorService;
    private readonly ILogger<SyncService> _logger;
    private readonly IConfiguration _configuration;

    public SyncService(ChiaService chiaService,
                        MirrorService mirrorService,
                        ILogger<SyncService> logger,
                        IConfiguration configuration) =>
            (_chiaService, _mirrorService, _logger, _configuration) = (chiaService, mirrorService, logger, configuration);

    public async Task SyncSubscriptions(CancellationToken stoppingToken)
    {
        var dataLayer = await _chiaService.GetDataLayer(stoppingToken);
        var xchWallet = await _chiaService.GetWallet(_configuration.GetValue<uint>("DlMirrorSync:XchWalletId", 1), stoppingToken);
        if (dataLayer is not null && xchWallet is not null)
        {
            try
            {
                var addMirrorAmount = _configuration.GetValue<ulong>("DlMirrorSync:AddMirrorAmount", 300000000);
                var fee = await _chiaService.GetFee(addMirrorAmount, stoppingToken);

                // make sure we clean up the rpc clients
                using var dlClient = dataLayer.RpcClient;
                using var walletClient = xchWallet.WalletProxy.RpcClient;

                var subscriptions = await dataLayer.Subscriptions(stoppingToken);

                await foreach (var id in _mirrorService.FetchLatest(stoppingToken))
                {
                    if (!subscriptions.Contains(id))
                    {
                        _logger.LogInformation("Subscribing to mirror {id}", id);
                        await dataLayer.Subscribe(id, Enumerable.Empty<string>(), stoppingToken);
                    }

                    var mirrors = await dataLayer.GetMirrors(id, stoppingToken);
                    // add any mirrors that aren't already ours
                    if (!mirrors.Any(m => m.Ours))
                    {
                        var balance = await xchWallet.GetBalance(stoppingToken);
                        if (addMirrorAmount + fee < balance.SpendableBalance)
                        {
                            await dataLayer.AddMirror(id, addMirrorAmount, Enumerable.Empty<string>(), fee, stoppingToken);
                        }
                        else
                        {
                            _logger.LogWarning("Insufficient funds to add mirror {id}. Balance={SpendableBalance}, Cost={addMirrorAmount}, Fee={fee}", id, balance.SpendableBalance, addMirrorAmount, fee);
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
}