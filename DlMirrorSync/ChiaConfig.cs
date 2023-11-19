using chia.dotnet;

namespace DlMirrorSync;

/// <summary>
/// Provides methods for managing chia connection details.
/// </summary>
public sealed class ChiaConfig
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChiaConfig> _logger;

    public ChiaConfig(ILogger<ChiaConfig> logger, IConfiguration configuration) =>
            (_logger, _configuration) = (logger, configuration);

    private string GetConfigPath()
    {
        // first see if we have a config file path from the command line
        var configPath = _configuration.GetValue("chia_config_path", "");
        if (!string.IsNullOrEmpty(configPath))
        {
            return configPath;
        }

        // then see if we have a config file path in the appsettings.json
        configPath = _configuration.GetValue("App:chia_config_path", "");
        if (!string.IsNullOrEmpty(configPath))
        {
            return configPath;
        }

        return "";
    }

    private Config GetConfig()
    {
        // first see if we have a config file path in the appsettings.json
        var configPath = GetConfigPath();
        if (!string.IsNullOrEmpty(configPath))
        {
            _logger.LogInformation("Using chia config {configPath}", configPath);

            return Config.Open(configPath);
        }

        // if not use the chia default '~/.chia/mainnet/config/config.yaml'
        _logger.LogInformation("Using default chia config");

        return Config.Open();
    }

    public IEnumerable<string> GetEndpointNames() => new string[] { "data_layer", "wallet", "full_node" };

    public EndpointInfo GetEndpoint(string name)
    {
        // first check user secrets for the connection
        // https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0&tabs=windows
        var endpointUri = _configuration.GetValue($"{name}_uri", "")!;
        if (!string.IsNullOrEmpty(endpointUri))
        {
            _logger.LogInformation("Connecting to {endpointUri}", endpointUri);
            return new EndpointInfo()
            {
                Uri = new Uri(endpointUri),
                // when stored in an environment variable the newlines might be escaped
                Cert = _configuration.GetValue($"{name}_cert", "")!.Replace("\\n", "\n"),
                Key = _configuration.GetValue($"{name}_key", "")!.Replace("\\n", "\n")
            };
        }
        else
        {
            // if not present see if we can get it from the config file
            return GetConfig().GetEndpoint(name);
        }
    }
}

