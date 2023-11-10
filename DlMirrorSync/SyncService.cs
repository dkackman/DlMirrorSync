using chia.dotnet;

namespace DlMirrorSync;

public sealed class SyncService
{
    private readonly MirrorService _mirrorService;
    private readonly ILogger<SyncService> _logger;
    public SyncService(MirrorService mirrorService, ILogger<SyncService> logger) =>
            (_mirrorService, _logger) = (mirrorService, logger);

    public async Task SyncSubscriptions(CancellationToken stoppingToken)
    {
        var dataLayer = await GetDataLayer(stoppingToken);
        if (dataLayer is not null)
        {
            foreach (var (id, urls) in await _mirrorService.FetchLatest(stoppingToken))
            {
                await dataLayer.Subscribe(id, urls, stoppingToken);
            }
        }
    }

    private async Task<DataLayerProxy?> GetDataLayer(CancellationToken stoppingToken)
    {
        try
        {
            // "ui" get's the same daemon that the electron ui uses
            // which is usually but not always the self hosted daemon
            var endpoint = Config.Open().GetEndpoint("ui");
            using var rpcClient = new WebSocketRpcClient(endpoint);
            await rpcClient.Connect();

            var daemon = new DaemonProxy(rpcClient, "DlMirrorSync");
            await daemon.RegisterService();

            return daemon.CreateProxyFrom<DataLayerProxy>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
            return null;
        }
    }
}