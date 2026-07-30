using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Sorbate.Scraping;

namespace Sorbate.CLI;

class Program {
    static async Task Main(string[] args) {
        var inMemorySettings = new Dictionary<string, string> {
            {"Scraping:SteamApiKey", ""},
            {"Scraping:SteamCmdPath", "steamcmd/steamcmd.exe"},
            {"Scraping:SteamWriteDirectory", "write_dir/"},
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        
        HttpClient client = new();
        
        SteamScraper scraper = new SteamScraper(client, configuration);

        var a = await scraper.ScrapeLatest();

        Console.WriteLine(a);
    }
}