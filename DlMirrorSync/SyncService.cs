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
            using var rpc = dataLayer.RpcClient;
            var subscriptions = await dataLayer.Subscriptions(stoppingToken);
            await foreach (var id in _mirrorService.FetchLatest(stoppingToken))
            {
                _logger.LogInformation("Subscribing to mirror {id}", id);

                if (!subscriptions.Contains(id))
                {
                    await dataLayer.Subscribe(id, Enumerable.Empty<string>(), stoppingToken);
                }
                //await dataLayer.AddMirror(id, 0, Enumerable.Empty<string>(), 0, stoppingToken);
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
            _logger.LogInformation("Connecting to chia daemon at {Uri}", endpoint.Uri);
            var rpcClient = new WebSocketRpcClient(endpoint);
            await rpcClient.Connect(stoppingToken);

            var daemon = new DaemonProxy(rpcClient, "DlMirrorSync");
            await daemon.RegisterService(stoppingToken);

            return daemon.CreateProxyFrom<DataLayerProxy>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
            return null;
        }
    }
}