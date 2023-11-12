using chia.dotnet;

namespace DlMirrorSync;

public sealed class ChiaService
{
    private readonly string? _configFilePath;
    private readonly ILogger<ChiaService> _logger;
    private readonly IConfiguration _configuration;

    public ChiaService(string? configFilePath, ILogger<ChiaService> logger, IConfiguration configuration) =>
            (_configFilePath, _logger, _configuration) = (configFilePath, logger, configuration);

    private Config GetConfig()
    {
        if (_configFilePath is not null)
        {
            _logger.LogInformation("Using config file at {Path}", _configFilePath);
            return Config.Open(_configFilePath);
        }

        return Config.Open();
    }

    public async Task<ulong> GetFee(ulong cost, CancellationToken stoppingToken)
    {
        try
        {
            var endpoint = GetConfig().GetEndpoint("full_node");
            using var rpcClient = new HttpRpcClient(endpoint);
            var fullNode = new FullNodeProxy(rpcClient, "DlMirrorSync");
            int[] targetTimes = { 300 }; // five minutes
            var fee = await fullNode.GetFeeEstimate(cost, targetTimes, stoppingToken);
            return fee.estimates.First();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
            return _configuration.GetValue<ulong>("DlMirrorSync:DefaultFee", 500000);
        }
    }

    public async Task<Wallet?> GetWallet(uint walletId, CancellationToken stoppingToken)
    {
        try
        {
            var endpoint = GetConfig().GetEndpoint("wallet");
            var rpcClient = new HttpRpcClient(endpoint);
            var wallet = new WalletProxy(rpcClient, "DlMirrorSync");

            await wallet.HealthZ(stoppingToken);
            return new Wallet(walletId, wallet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
            return null;
        }
    }

    public async Task<DataLayerProxy?> GetDataLayer(CancellationToken stoppingToken)
    {
        try
        {
            // "ui" get's the same daemon that the electron ui uses
            // which is usually but not always the self hosted daemon
            // so get the daemon and use the host name for the data layer uri
            var endpoint = GetConfig().GetEndpoint("data_layer");

            _logger.LogInformation("Connecting to data layer at {Uri}", endpoint.Uri);
            var rpcClient = new HttpRpcClient(endpoint);
            var dl = new DataLayerProxy(rpcClient, "DlMirrorSync");
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