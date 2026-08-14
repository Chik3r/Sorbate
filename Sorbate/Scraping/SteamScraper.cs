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
        $"https://api.steampowered.com/IPublishedFileService/QueryFiles/v1/?key={{0}}&query_type=21&cursor={{1}}&numperpage=50&appid={TmlAppId}&return_short_description=true";
    
    private readonly HttpClient _http;
    private readonly IStorage _storage;
    private readonly ILogger<SteamScraper> _logger;
    private readonly string? _steamApiKey;
    private readonly string? _steamCmdPath;
    private readonly string? _steamWriteDirectory;
    private string RealSteamWriteDirectory => Path.Combine(Path.GetDirectoryName(_steamCmdPath) ?? string.Empty, _steamWriteDirectory ?? string.Empty);

    private static readonly SemaphoreSlim SteamCmdSemaphore = new(1, 1);
    
    public string SourceName => "Steam";

    public SteamScraper(HttpClient http, IConfiguration configuration, IStorage storage, ILogger<SteamScraper> logger) {
        _http = http;
        _storage = storage;
        _logger = logger;
        _steamApiKey = configuration.GetValue<string>("Scraping:SteamApiKey");
        _steamCmdPath = configuration.GetValue<string>("Scraping:SteamCmdPath");
        _steamWriteDirectory = configuration.GetValue<string>("Scraping:SteamWriteDirectory");

        if (_steamApiKey is null) {
            _logger.LogError("Steam API Key not found");
        }

        if (_steamCmdPath is null || !File.Exists(_steamCmdPath)) {
            _steamCmdPath = null;
            _logger.LogError("SteamCmdPath not found or invalid");
        }

        if (_steamWriteDirectory is null || !Directory.Exists(RealSteamWriteDirectory)) {
            _steamWriteDirectory = null;
            _logger.LogError("SteamWriteDirectory not found or invalid");
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
            _logger.LogWarning("Steam Api Key has not been set.");
            return false;
        }

        if (_steamCmdPath is null || _steamWriteDirectory is null || !File.Exists(_steamCmdPath)) {
            _logger.LogWarning("SteamCMD Path or write directory has not been set.");
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
                _logger.LogWarning("Steam API did not succeed, status code: {StatusCode}", response.StatusCode);
                _logger.LogDebug("API request used url: {ApiUrl}", api.Replace(_steamApiKey!, "<key>"));
                yield break;
            }

            // Get JSON from response and parse it
            SteamResponseRoot? steamResponseRoot =
                JsonSerializer.Deserialize<SteamResponseRoot>(await response.Content.ReadAsStringAsync(token));
            if (steamResponseRoot is null) {
                _logger.LogWarning("Failed to deserialize Steam response");
                yield break;
            }
            
            SteamResponse steamResponse = steamResponseRoot.Response;
            if (steamResponse.PublishedFileDetails is null) {
                // If the current cursor and next cursor match, then we have gone through all results and can ignore the null value.
                if (steamResponse.NextCursor != cursor) {
                    _logger.LogInformation("PublishedFileDetails was null, with cursor: {cursor}", cursor);
                }
                
                yield break;
            }
            
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
        const int sizeLimit = 2 * 1000 * 1000 * 1000; // 2 GB
        int sumSize = 0;
        Dictionary<string, int> idToSteamTime = new();
        await foreach (PublishedFileDetail workshopItem in workshopItems.WithCancellation(token)) {
            string id = workshopItem.PublishedFileId;

            if (!int.TryParse(workshopItem.FileSize, out int fileSize)) {
                _logger.LogWarning("Failed to get file size of mod {id}, value: {fileSize}. Skipping mod.", id, workshopItem.FileSize);
                continue;
            }
            
            // Skip files that are already in storage
            if (await _storage.GetLastUpdateTimestamp(id) == SteamTimeToDateTime(workshopItem.TimeUpdated)) {
                _logger.LogTrace("Skipping mod {id}, already in storage.", id);
                continue;
            }

            // If we already have some files in the "queue" and adding this file would bring us over the sizeLimit,
            // then return the argument we have so far, clean up and then continue.
            // Additionally, only download up to 20 mods at a time (just in case steam gets angry or something).
            if ((sumSize != 0 && (sumSize + fileSize) >= sizeLimit) || idToSteamTime.Count >= 20) {
                // Over the size limit, fetch the current queue
                argBuilder.Append(suffixArgument);
                yield return (argBuilder.ToString(), new Dictionary<string, int>(idToSteamTime));
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
            // Note: Use download_item instead of workshop_download_item so that we avoid the workshop system.
            // Otherwise, steam will try to redownload old deleted files later, causing issues.
            // extra note, validate isn't an option for download_item
            // Note 2: download_item fails for some items with the message "Depot download failed : workshop item not found (Missing configuration)"
            // We will go back to workshop_download_item, and delete the /workshop/ folder every time in hopes that the workshop cache is deleted
            // Ideally this will prevent the issues we had with workshop_download_item 

            await SteamCmdSemaphore.WaitAsync(token);
            
            _logger.LogInformation("Downloading mods from Steam using SteamCMD");
            _logger.LogDebug("SteamCMD argument: {argument}", argument);
            ProcessStartInfo procInfo;
            if (OperatingSystem.IsWindows()) {
                procInfo = new ProcessStartInfo {
                    Arguments = argument,
                    FileName = _steamCmdPath!,
                    WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(_steamCmdPath!)),
                    // UseShellExecute = true
                };
            }
            else {
                procInfo = new ProcessStartInfo {
                    FileName = "bash",
                    Arguments = $"{_steamCmdPath!} {argument}",
                    WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(_steamCmdPath!)),
                    UseShellExecute = false,
                };
            }

            Process? steamCmd = Process.Start(procInfo);
            if (steamCmd is null) {
                _logger.LogError("Failed to start SteamCMD");
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
        string searchFolder = Path.Combine(RealSteamWriteDirectory, "steamapps/workshop/content", $"app_{TmlAppId}");
        if (!Directory.Exists(searchFolder)) {
            _logger.LogError("Failed to find download directory {directory}", searchFolder);
            return [];
        }

        IEnumerable<string> tmodFiles =
            Directory.EnumerateFiles(searchFolder, "*.tmod", SearchOption.AllDirectories);

        List<ModRecord> modRecords = [];
        foreach (string tmodFile in tmodFiles) {
            // The signature will be erased when writing the tmod file again
            await using FileStream fs = File.OpenRead(tmodFile);
            SerializableTmodFile tmod = SerializableTmodFile.FromStream(fs);

            // Folder name should be item_<id>, we try to get the <id> part
            string workshopId = Directory.GetParent(tmodFile)!.Parent!.Name.Split('_').ElementAtOrDefault(1) ?? TmlAppId;
            // For really old mods on workshop
            if (workshopId == TmlAppId) 
                workshopId = Directory.GetParent(tmodFile)!.Name.Split('_').ElementAtOrDefault(1) ?? "null"; 
            
            if (!fileIdTimestampMapping.TryGetValue(workshopId, out int timestamp)) {
                // TODO: log warn or something, this would be caused if files were not deleted after downloading them
                _logger.LogInformation("Failed to find timestamp for {id}", workshopId);
                _logger.LogDebug("Dump of FileId mapping, {dump}", string.Join(" | ", fileIdTimestampMapping));
            }

            ModRecord record = new() {
                Timestamp = SteamTimeToDateTime(timestamp),
                Source = SourceName,
                PublishedFileId = workshopId,
                Hash = tmod.Hash,
                Data = tmod,
            };

            // TODO: DEBUG
            _logger.LogDebug("Found mod file at {path}", tmodFile);
            modRecords.Add(record);
        }

        // Delete files after we have read them, as they should be loaded in memory        
        Directory.Delete(Path.Combine(RealSteamWriteDirectory, "steamapps/workshop"), true);

        return modRecords;
    }

    private static DateTime SteamTimeToDateTime(int timestamp) {
        return DateTime.UnixEpoch.AddSeconds(timestamp);
    }
}