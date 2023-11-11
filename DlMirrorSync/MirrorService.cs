namespace DlMirrorSync;

using System.Net.Http;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public sealed class MirrorService
{
    private readonly ILogger<MirrorService> _logger;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        }
    };

    public MirrorService(
        ILogger<MirrorService> logger,
        IConfiguration configuration) =>
        (_logger, _configuration) = (logger, configuration);

    public async IAsyncEnumerable<string> FetchLatest([EnumeratorCancellation] CancellationToken stoppingToken)
    {
        var uri = _configuration["DlMirrorSync:MirrorServiceUri"] ?? throw new InvalidOperationException("Missing MirrorServiceUri");

        _logger.LogInformation("Fetching latest mirrors from {uri}", uri);
        using var httpClient = new HttpClient();
        var currentPage = 1;
        var totalPages = 0; // we won't know actual total pages until we get the first page

        do
        {
            var page = await GetPage(httpClient, uri, currentPage, stoppingToken);
            totalPages = page.TotalPages;

            foreach (var singleton in page.Mirrors)
            {
                yield return singleton.SingletonId;
            }
            System.Diagnostics.Debug.WriteLine($"Page {currentPage} of {totalPages}");

            currentPage++;
        } while (currentPage <= totalPages && !stoppingToken.IsCancellationRequested);
    }

    private async Task<PageRecord> GetPage(HttpClient httpClient, string uri, int currentPage, CancellationToken stoppingToken)
    {
        try
        {
            using var response = await httpClient.GetAsync($"{uri}?page={currentPage}", stoppingToken);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync(stoppingToken);
            return JsonConvert.DeserializeObject<PageRecord>(responseBody, _settings) ?? throw new InvalidOperationException("Failed to fetch mirrors");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
            // this is not fatal to the process, so return an empty page
            return new PageRecord();
        }
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