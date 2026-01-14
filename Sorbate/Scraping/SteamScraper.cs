using System.Text.Json;
using Sorbate.Data;

namespace Sorbate.Scraping;

public class SteamScraper : IScraper {
    private const string TmlAppId = "1281930";
    private const string ApiUrl =
        $"https://api.steampowered.com/IPublishedFileService/QueryFiles/v1/?key={{0}}&query_type=1&cursor={{1}}&numperpage=50&appid={TmlAppId}&return_short_description=true";
    
    private readonly HttpClient _http;
    private readonly string? _steamApiKey;
    
    public string SourceName => "Steam";

    public SteamScraper(HttpClient http, IConfiguration configuration) {
        _http = http;
        _steamApiKey = configuration.GetValue<string>("Scraping:SteamApiKey");

        if (_steamApiKey is null) {
            // TODO: Log no API key (ERROR)
            Console.WriteLine("Steam API Key not found");
        }
    }
    
    public async Task<IEnumerable<ModRecord>> ScrapeLatest() {
        // TODO: do it
        if (_steamApiKey is null) {
            // TODO: use proper logging (INFO maybe)
            Console.WriteLine("Steam API Key not found");
            return [];
        }

        string api = string.Format(ApiUrl, _steamApiKey, "*");
        // Make request to API
        HttpResponseMessage response = await _http.GetAsync(api);
        if (!response.IsSuccessStatusCode) {
            // TODO: logging (WARN)
            Console.WriteLine("Steam API returned {0}", response.StatusCode);
            return [];
        }

        // Get JSON from response and parse it
        SteamResponseRoot? steamResponseRoot = JsonSerializer.Deserialize<SteamResponseRoot>(await response.Content.ReadAsStringAsync());
        if (steamResponseRoot is null) {
            // TODO: logging (WARN)
            Console.WriteLine("Failed to deserialize {0}", response);
            return [];
        }
        
        SteamResponse steamResponse = steamResponseRoot.Response;
        foreach (PublishedFileDetail fileDetail in steamResponse.PublishedFileDetails) {
            string id = fileDetail.PublishedFileId;
            
            
            // TODO: maybe filter somehow to see if we already have this upload?
            
            // Use ID to access SteamCMD and download mod
            // Command is  './steamcmd.exe +login anonymous +workshop_download_item {TmlAppId} {id} validate +quit'
            // File will be saved to './steamapps/workshop/content/{TmlAppId}/{id}/'
        }

        return [
            new ModRecord {
                Timestamp = DateTime.UtcNow,
                Source = $"{SourceName}-mod_id",
                Hash = new byte[20],
                FileName = "test"
            }
        ];
    }

    public  async Task<IEnumerable<ModRecord>> ScrapeHistorical() {
        // TODO: do it
        await Task.Delay(20);
        
        return [
            new ModRecord {
                Timestamp = DateTime.UtcNow.AddDays(-1),
                Source = $"{SourceName}-mod_id_historical",
                Hash = new byte[20],
                FileName = "historical"
            }
        ];
    }
}