using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using Sorbate.Data;
using Tomat.FNB.TMOD;

namespace Sorbate.Scraping;

public class SteamScraper : IScraper {
    private const string TmlAppId = "1281930";
    private const string ApiUrl =
        $"https://api.steampowered.com/IPublishedFileService/QueryFiles/v1/?key={{0}}&query_type=1&cursor={{1}}&numperpage=3&appid={TmlAppId}&return_short_description=true";
    
    private readonly HttpClient _http;
    private readonly string? _steamApiKey;
    private readonly string? _steamCmdPath;
    private readonly string? _steamWriteDirectory;
    private string RealSteamWriteDirectory => Path.Combine(Path.GetDirectoryName(_steamCmdPath) ?? string.Empty, _steamWriteDirectory ?? string.Empty);
    
    public string SourceName => "Steam";

    public SteamScraper(HttpClient http, IConfiguration configuration) {
        _http = http;
        _steamApiKey = configuration.GetValue<string>("Scraping:SteamApiKey");
        _steamCmdPath = configuration.GetValue<string>("Scraping:SteamCmdPath");
        _steamWriteDirectory = configuration.GetValue<string>("Scraping:SteamWriteDirectory");

        if (_steamApiKey is null) {
            // TODO: Log no API key (ERROR)
            Console.WriteLine("Steam API Key not found");
        }

        if (_steamCmdPath is null || !File.Exists(_steamCmdPath)) {
            // var a = Environment.CurrentDirectory;
            // TODO: Log no SteamCMD (ERROR)
            _steamCmdPath = null;
            Console.WriteLine("SteamCMD Path not found or invalid");
        }

        if (_steamWriteDirectory is null || !Directory.Exists(RealSteamWriteDirectory)) {
            // TODO: log error
            _steamWriteDirectory = null;
            Console.WriteLine("SteamWriteDirectory not found or invalid");
        }
    }

    public async Task<IEnumerable<ModRecord>> ScrapeLatest(CancellationToken token = default) {
        if (_steamApiKey is null) {
            // TODO: use proper logging (INFO maybe)
            Console.WriteLine("Steam API Key not found");
            return [];
        }

        if (_steamCmdPath is null || _steamWriteDirectory is null) {
            // TODO: logging
            Console.WriteLine("SteamCMD Path or write directory not found");
            return [];
        }

        string api = string.Format(ApiUrl, _steamApiKey, "*");
        // Make request to API
        HttpResponseMessage response = await _http.GetAsync(api, token);
        if (!response.IsSuccessStatusCode) {
            // TODO: logging (WARN)
            Console.WriteLine("Steam API returned {0}", response.StatusCode);
            return [];
        }

        // Get JSON from response and parse it
        SteamResponseRoot? steamResponseRoot =
            JsonSerializer.Deserialize<SteamResponseRoot>(await response.Content.ReadAsStringAsync(token));
        if (steamResponseRoot is null) {
            // TODO: logging (WARN)
            Console.WriteLine("Failed to deserialize {0}", response);
            return [];
        }

        List<ModRecord> modRecords = [];
        SteamResponse steamResponse = steamResponseRoot.Response;
        foreach (PublishedFileDetail fileDetail in steamResponse.PublishedFileDetails) {
            string id = fileDetail.PublishedFileId;

            // TODO: maybe filter somehow to see if we already have this upload?
            // Ok, idea, do an initial loop to filter for IDs that are not in DB or that have been updated recently
            // Remove every folder for a mod not in PublishedFileDetails (so we remove any old mods) or maybe not in our filtered list
            // Then, with this list of IDs, create the SteamCMD command "steamcmd +login anon +workshop_download ... +workshop.... +quit"
            // with a +workshop_download_item for each mod
            // Finally, get the .tmod files and process them etc

            // Use ID to access SteamCMD and download mod
            // Command is  './steamcmd.exe +login anonymous +workshop_download_item {TmlAppId} {id} validate +quit'
            // File will be saved to './steamapps/workshop/content/{TmlAppId}/{id}/'

            // TODO: logging (DEBUG)
            Console.WriteLine("Downloading mod {0} from Steam", id);
            ProcessStartInfo procInfo = new() {
                Arguments =
                    $"+force_install_dir {_steamWriteDirectory} +login anonymous +workshop_download_item {TmlAppId} {id} validate +quit",
                FileName = _steamCmdPath,
                WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(_steamCmdPath)),
                // UseShellExecute = true
            };

            Process? steamCmd = Process.Start(procInfo);
            if (steamCmd is null) {
                // TODO: log WARN
                Console.WriteLine("Failed to start SteamCMD");
                return [];
            }

            await steamCmd.WaitForExitAsync(token);

            // Find the .tmod file
            string searchFolder =
                Path.Combine(RealSteamWriteDirectory, "steamapps/workshop/content", $"{TmlAppId}", $"{id}");
            if (!Directory.Exists(searchFolder)) {
                // TODO: warn
                Console.WriteLine("Failed to find download directory {0}", searchFolder);
                return [];
            }

            IEnumerable<string> tmodFiles =
                Directory.EnumerateFiles(searchFolder, "*.tmod", SearchOption.AllDirectories);

            foreach (string tmodFile in tmodFiles) {
                // TODO: erase the tmod's Signature field
                await using FileStream fs = File.OpenRead(tmodFile);
                SerializableTmodFile tmod = SerializableTmodFile.FromStream(fs);

                ModRecord record = new() {
                    Timestamp = DateTime.UnixEpoch.AddSeconds(fileDetail.TimeUpdated),
                    Source = SourceName,
                    Hash = tmod.Hash,
                    Data = tmod,
                };

                // TODO: DEBUG
                Console.WriteLine("Found : {0}", tmodFile);
                modRecords.Add(record);
            }
        }

        return modRecords;
    }

    public async Task<IEnumerable<ModRecord>> ScrapeHistorical(CancellationToken token = default) {
        // TODO: do it
        await Task.Delay(20, token);
        
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