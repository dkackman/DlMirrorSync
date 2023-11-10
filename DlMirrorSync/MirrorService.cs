namespace DlMirrorSync;

public sealed class MirrorService
{
    private readonly ILogger<MirrorService> _logger;
    private readonly IConfiguration _configuration;

    public MirrorService(
        ILogger<MirrorService> logger,
        IConfiguration configuration) =>
        (_logger, _configuration) = (logger, configuration);

    public async Task<IEnumerable<(string id, IEnumerable<string> urls)>> FetchLatest(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        return new List<(string id, IEnumerable<string> urls)>();
    }
}