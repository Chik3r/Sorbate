using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Sorbate.Data;
using Tomat.FNB.TMOD;

namespace Sorbate.Scraping;

public class SteamScraper : IScraper {
    private const string TmlAppId = "1281930";
    // Query type 21 means sort by last updated
    private const string ApiUrl =
        $"https://api.steampowered.com/IPublishedFileService/QueryFiles/v1/?key={{0}}&query_type=21&cursor={{1}}&numperpage=3&appid={TmlAppId}&return_short_description=true";
    
    private readonly HttpClient _http;
    private readonly IStorage _storage;
    private readonly string? _steamApiKey;
    private readonly string? _steamCmdPath;
    private readonly string? _steamWriteDirectory;
    private string RealSteamWriteDirectory => Path.Combine(Path.GetDirectoryName(_steamCmdPath) ?? string.Empty, _steamWriteDirectory ?? string.Empty);

    private static readonly SemaphoreSlim SteamCmdSemaphore = new(1, 1);
    
    public string SourceName => "Steam";

    public SteamScraper(HttpClient http, IConfiguration configuration, IStorage storage) {
        _http = http;
        _storage = storage;
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

    public async Task<IAsyncEnumerable<ModRecord>> ScrapeLatest(CancellationToken token = default) {
        if (!CheckValidSteamParameters()) return AsyncEnumerable.Empty<ModRecord>();

        IAsyncEnumerable<PublishedFileDetail> publishedFiles = ListWorkshopItems(token);

        return DownloadWorkshopItems(publishedFiles, token);
    }

    public async Task<IAsyncEnumerable<ModRecord>> ScrapeHistorical(CancellationToken token = default) {
        if (!CheckValidSteamParameters()) return AsyncEnumerable.Empty<ModRecord>();

        IAsyncEnumerable<PublishedFileDetail> publishedFiles = ListWorkshopItems(token, true);

        return DownloadWorkshopItems(publishedFiles, token);
    }
    
    /// <summary>
    /// Ensures that the Steam API key, the SteamCMD path and the SteamCMD write directory are valid.
    /// </summary>
    /// <returns>Returns true if they pass a basic check.</returns>
    private bool CheckValidSteamParameters() {
        if (_steamApiKey is null) {
            // TODO: use proper logging (INFO maybe)
            Console.WriteLine("Steam API Key not found");
            return false;
        }

        if (_steamCmdPath is null || _steamWriteDirectory is null || !File.Exists(_steamCmdPath)) {
            // TODO: logging
            Console.WriteLine("SteamCMD Path or write directory not found");
            return false;
        }

        return true;
    }

    private async IAsyncEnumerable<PublishedFileDetail> ListWorkshopItems([EnumeratorCancellation] CancellationToken token, bool multiplePages = false) {
        string cursor = "*";

        while (true) {
            token.ThrowIfCancellationRequested();
            
            string api = string.Format(ApiUrl, _steamApiKey, cursor);
            
            // Make request to API
            HttpResponseMessage response = await _http.GetAsync(api, token);
            if (!response.IsSuccessStatusCode) {
                // TODO: logging (WARN)
                Console.WriteLine("Steam API returned {0}", response.StatusCode);
                yield break;
            }

            // Get JSON from response and parse it
            SteamResponseRoot? steamResponseRoot =
                JsonSerializer.Deserialize<SteamResponseRoot>(await response.Content.ReadAsStringAsync(token));
            if (steamResponseRoot is null) {
                // TODO: logging (WARN)
                Console.WriteLine("Failed to deserialize {0}", response);
                yield break;
            }
            
            SteamResponse steamResponse = steamResponseRoot.Response;
            foreach (PublishedFileDetail fileDetail in steamResponse.PublishedFileDetails) {
                yield return fileDetail;
            }
            
            // Cursor can contain special characters such as +
            cursor = Uri.EscapeDataString(steamResponse.NextCursor);

            if (!multiplePages) break;
            if (steamResponse.PublishedFileDetails.Count == 0) break;
        }
    }

    private async IAsyncEnumerable<(string argument, Dictionary<string, int> idToSteamTime)> PrepareDownloadArguments(IAsyncEnumerable<PublishedFileDetail> workshopItems, [EnumeratorCancellation] CancellationToken token) {
        string prefixArgument = $"+force_install_dir {_steamWriteDirectory} +login anonymous";
        const string suffixArgument = " +quit";
        StringBuilder argBuilder = new();
        argBuilder.Append(prefixArgument);

        // Only download up to sizeLimit amount of data at a time.
        // This is needed because all the downloaded data will be loaded into RAM later on.
        const int sizeLimit = 2 * 1000 * 1000; // 2 GB
        int sumSize = 0;
        Dictionary<string, int> idToSteamTime = new();
        await foreach (PublishedFileDetail workshopItem in workshopItems.WithCancellation(token)) {
            string id = workshopItem.PublishedFileId;

            if (!int.TryParse(workshopItem.FileSize, out int fileSize)) {
                Console.WriteLine($"Failed to get file size of mod {id}, value: {workshopItem.FileSize}. Skipping mod.");
                continue;
            }
            
            // Skip files that are already in storage
            if (await _storage.GetLastUpdateTimestamp(id) == SteamTimeToDateTime(workshopItem.TimeUpdated)) {
                Console.WriteLine($"Skipping mod {id}, already in storage.");
                continue;
            }

            // If we already have some files in the "queue" and adding this file would bring us over the sizeLimit,
            // then return the argument we have so far, clean up and then continue.
            // Additionally, only download up to 20 mods at a time (just in case steam gets angry or something).
            if ((sumSize != 0 && (sumSize + fileSize) >= sizeLimit) || idToSteamTime.Count >= 20) {
                // Over the size limit, fetch the current queue
                argBuilder.Append(suffixArgument);
                yield return (argBuilder.ToString(), idToSteamTime);
                sumSize = 0;
                argBuilder.Clear();
                argBuilder.Append(prefixArgument);
                idToSteamTime.Clear();
            }

            sumSize += fileSize;
            argBuilder.Append($" +workshop_download_item {TmlAppId} {id} validate");
            idToSteamTime.Add(id, workshopItem.TimeUpdated);
        }

        if (sumSize == 0) yield break;

        argBuilder.Append(suffixArgument);
        yield return (argBuilder.ToString(), idToSteamTime);
    }

    private async IAsyncEnumerable<ModRecord> DownloadWorkshopItems(
        IAsyncEnumerable<PublishedFileDetail> workshopItems,
        [EnumeratorCancellation] CancellationToken token) {
        await foreach ((string argument, Dictionary<string, int> idToSteamTime) in 
                       PrepareDownloadArguments(workshopItems, token)) {
            
            // Start SteamCMD, download the mods in the argument, and then return the tmod files in memory.
            
            // Use ID to access SteamCMD and download mod
            // Command is  './steamcmd.exe +login anonymous +workshop_download_item {TmlAppId} {id} validate +quit'
            // File will be saved to './steamapps/workshop/content/{TmlAppId}/{id}/'

            await SteamCmdSemaphore.WaitAsync(token);
            
            // TODO: logging (DEBUG)
            Console.WriteLine("Downloading mods from Steam");
            ProcessStartInfo procInfo = new() {
                Arguments = argument,
                FileName = _steamCmdPath,
                WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(_steamCmdPath!)),
                // UseShellExecute = true
            };

            Process? steamCmd = Process.Start(procInfo);
            if (steamCmd is null) {
                // TODO: log WARN
                Console.WriteLine("Failed to start SteamCMD");
                SteamCmdSemaphore.Release();
                yield break;
            }
            
            await steamCmd.WaitForExitAsync(token);

            IEnumerable<ModRecord> downloadedFiles = await ListDownloadedFiles(idToSteamTime);
            foreach (ModRecord record in downloadedFiles) {
                yield return record;
            }

            SteamCmdSemaphore.Release();
        }
    }
    
    private async Task<List<ModRecord>> ListDownloadedFiles(Dictionary<string, int> fileIdTimestampMapping) {
        // Find the .tmod file
        string searchFolder = Path.Combine(RealSteamWriteDirectory, "steamapps/workshop/content", $"{TmlAppId}");
        if (!Directory.Exists(searchFolder)) {
            // TODO: warn
            Console.WriteLine("Failed to find download directory {0}", searchFolder);
            return [];
        }

        IEnumerable<string> tmodFiles =
            Directory.EnumerateFiles(searchFolder, "*.tmod", SearchOption.AllDirectories);

        List<ModRecord> modRecords = [];
        foreach (string tmodFile in tmodFiles) {
            // The signature will be erased when writing the tmod file again
            await using FileStream fs = File.OpenRead(tmodFile);
            SerializableTmodFile tmod = SerializableTmodFile.FromStream(fs);

            string workshopId = Directory.GetParent(tmodFile)!.Parent!.Name;
            if (!fileIdTimestampMapping.TryGetValue(workshopId, out int timestamp)) {
                // TODO: log warn or something, this would be caused if files were not deleted after downloading them
                Console.WriteLine("Failed to find timestamp for {0}", workshopId);
            }

            ModRecord record = new() {
                Timestamp = SteamTimeToDateTime(timestamp),
                Source = SourceName,
                PublishedFileId = workshopId,
                Hash = tmod.Hash,
                Data = tmod,
            };

            // TODO: DEBUG
            Console.WriteLine("Found : {0}", tmodFile);
            modRecords.Add(record);
        }

        // Delete files after we have read them, as they should be loaded in memory        
        Directory.Delete(Path.Combine(RealSteamWriteDirectory, "steamapps/workshop/content"), true);

        return modRecords;
    }

    private static DateTime SteamTimeToDateTime(int timestamp) {
        return DateTime.UnixEpoch.AddSeconds(timestamp);
    }
}