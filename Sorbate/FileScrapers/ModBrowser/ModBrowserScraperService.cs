using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sorbate.Storage;

namespace Sorbate.FileScrapers.ModBrowser;

public partial class ModBrowserScraperService : BackgroundService {
    private const string BASE_BROWSER_URL = "http://javid.ddns.net/";
    private const string BROWSER_URL = BASE_BROWSER_URL + "/tModLoader/DirectModDownloadListing.php";
    
    private readonly Regex _modUrlRegex = ModUrlRegex();
    private readonly ILogger<ModBrowserScraperService> _logger;
    private readonly StorageHandler _storageHandler;
    private readonly HttpClient _client;
    private readonly string? _authToken;

    public ModBrowserScraperService(ILogger<ModBrowserScraperService> logger, HttpClient client,
        StorageHandler storageHandler, IConfiguration configuration) {
        _logger = logger;
        _storageHandler = storageHandler;
        _client = client;

        _authToken = configuration.GetSection("GitHub")["AuthToken"];
        if (!string.IsNullOrWhiteSpace(_authToken))
            return;
        logger.LogError("Missing GitHub authorization token");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        if (_authToken is null) {
            _logger.LogInformation("No GitHub key, stopping service");
            return;
        }
        
        _logger.LogInformation("Starting .tmod file search");

        HttpResponseMessage browserResponse = await _client.GetAsync(BROWSER_URL, stoppingToken);
        if (!browserResponse.IsSuccessStatusCode) {
            _logger.LogError("Failed to get mod list from browser, status code: {0}", browserResponse.StatusCode);
            return;
        }
        
        string browserContent = await browserResponse.Content.ReadAsStringAsync(stoppingToken);
        MatchCollection matches = _modUrlRegex.Matches(browserContent);
        if (matches.Count == 0) {
            _logger.LogWarning("No mods found in browser");
            return;
        }
        
        foreach (Match match in matches) {
            string modRelativeUrl = match.Groups[1].Value;
            _logger.LogInformation("Found mod relative url: {0}", modRelativeUrl);
        }
    }

    [GeneratedRegex("javid.ddns.net([^\">]+)\">", RegexOptions.Compiled)]
    private static partial Regex ModUrlRegex();
}