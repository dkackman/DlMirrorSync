namespace DlMirrorSync;

using System.Net.Http.Json;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public sealed class MirrorService
{
    private readonly ILogger<MirrorService> _logger;
    private readonly IConfiguration _configuration;

    public MirrorService(
        ILogger<MirrorService> logger,
        IConfiguration configuration) =>
        (_logger, _configuration) = (logger, configuration);

    public async IAsyncEnumerable<string> FetchLatest([EnumeratorCancellation] CancellationToken stoppingToken)
    {
        var uri = _configuration["DlMirrorSync:MirrorServiceUri"];
        _logger.LogInformation("Fetching latest mirrors from { uri}", uri);
        using var httpClient = new HttpClient();
        var pageIndex = 1;
        var totalPages = 1;
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        do
        {
            var response = await httpClient.GetAsync($"{uri}?page={pageIndex}", stoppingToken);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync(stoppingToken);
            var page = JsonConvert.DeserializeObject<PageRecord>(responseBody, settings) ?? throw new InvalidOperationException("Failed to fetch mirrors");
            totalPages = page.TotalPages;

            foreach (var singleton in page.Mirrors)
            {
                yield return singleton.SingletonId;
            }
            System.Diagnostics.Debug.WriteLine($"Page {pageIndex} of {totalPages}");

            pageIndex++;
        } while (pageIndex <= totalPages && !stoppingToken.IsCancellationRequested);
    }
}

public record PageRecord
{
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int TotalPages { get; init; }
    public IEnumerable<SingletonRef> Mirrors { get; init; } = new List<SingletonRef>();
}

public record SingletonRef
{
    public string SingletonId { get; init; } = string.Empty;
}