using System.Linq;
using chia.dotnet;

namespace DlMirrorSync;

public sealed class SyncService
{
    private readonly MirrorService _mirrorService;
    private readonly ILogger<SyncService> _logger;
    private readonly IConfiguration _configuration;

    public SyncService(MirrorService mirrorService, ILogger<SyncService> logger, IConfiguration configuration) =>
            (_mirrorService, _logger, _configuration) = (mirrorService, logger, configuration);

    public async Task SyncSubscriptions(CancellationToken stoppingToken)
    {
        var dataLayer = await GetDataLayer(stoppingToken);
        if (dataLayer is not null)
        {
            var addMirrorAmount = _configuration.GetValue<ulong>("DlMirrorSync:AddMirrorAmount", 300000000);
            var fee = await GetFee(addMirrorAmount, stoppingToken);

            using var rpc = dataLayer.RpcClient;
            var subscriptions = await dataLayer.Subscriptions(stoppingToken);

            await foreach (var id in _mirrorService.FetchLatest(stoppingToken))
            {
                if (!subscriptions.Contains(id))
                {
                    _logger.LogInformation("Subscribing to mirror {id}", id);
                    await dataLayer.Subscribe(id, Enumerable.Empty<string>(), stoppingToken);
                }

                var mirrors = await dataLayer.GetMirrors(id, stoppingToken);
                // TODO - this collection isn't ever empty even if mirror has not been added
                // so prolly don't fully understand this yet.
                if (!mirrors.Any())
                {
                    await dataLayer.AddMirror(id, addMirrorAmount, Enumerable.Empty<string>(), fee, stoppingToken);
                }
            }
        }
    }

    private async Task<ulong> GetFee(ulong cost, CancellationToken stoppingToken)
    {
        try
        {
            var endpoint = Config.Open().GetEndpoint("full_node");
            using var rpcClient = new HttpRpcClient(endpoint);
            var fullNode = new FullNodeProxy(rpcClient, "DlMirrorSync");
            int[] targetTimes = { 300 };
            var fee = await fullNode.GetFeeEstimate(cost, targetTimes, stoppingToken);
            return fee.estimates.First();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
            return _configuration.GetValue<ulong>("DlMirrorSync:DefaultFee", 500000);
        }
    }
    private async Task<DataLayerProxy?> GetDataLayer(CancellationToken stoppingToken)
    {
        try
        {
            // "ui" get's the same daemon that the electron ui uses
            // which is usually but not always the self hosted daemon
            // so get the daemon and use the host name for the data layer uri
            var endpoint = Config.Open().GetEndpoint("data_layer");

            _logger.LogInformation("Connecting to data layer at {Uri}", endpoint.Uri);
            var rpcClient = new HttpRpcClient(endpoint);
            var dl = new DataLayerProxy(rpcClient, "DlMirrorSync");
            // quick heartbeat validation 
            await dl.HealthZ(stoppingToken);
            return dl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
            return null;
        }
    }
}