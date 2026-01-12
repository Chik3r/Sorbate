using Sorbate.Data;

namespace Sorbate.Scraping;

public class SteamScraper : IScraper {
    private readonly HttpClient _http;
    
    public string SourceName => "Steam";

    public SteamScraper(HttpClient http) {
        _http = http;
    }
    
    public async Task<IEnumerable<ModRecord>> ScrapeLatest() {
        // TODO: do it
        await Task.Delay(20);


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