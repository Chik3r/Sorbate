using Sorbate.Storage.Models;
using Tomat.FNB.TMOD;

namespace Sorbate.Storage;

// TODO
// - Store the SHA1 hash of the tmod file, prevents repeated files by checking if they are already stored
// - logging
// - store the author of the mod

public class StorageHandler {
    private const string MOD_FILE_DIRECTORY = "tmod_files";
    
    private readonly HttpClient _client;

    public StorageHandler(HttpClient client) {
        _client = client;
    }
    
    public async Task StoreFile(Stream fileData, ModFile fileInfo, string outputDirectory) {
        Directory.CreateDirectory(outputDirectory);

        Guid fileGuid = Guid.NewGuid();
        fileInfo.Id = fileGuid;
        string fileName = Path.ChangeExtension(fileGuid.ToString(), ".tmod");
        string filePath = Path.Combine(outputDirectory, fileName);

        await using FileStream fileStream = File.Create(filePath);
        fileData.Position = 0;
        await fileData.CopyToAsync(fileStream);

        await using StorageContext db = new();
        
        db.Files.Add(fileInfo);
        await db.SaveChangesAsync();
    }

    public async Task StoreModFile(Stream modFile) => await StoreModFile(modFile, MOD_FILE_DIRECTORY);

    public async Task StoreModFile(Stream modFile, string outputDirectory) {
        if (!TmodFile.TryReadFromStream(modFile, out TmodFile? tmodFile))
            throw new Exception("Unable to read .tmod file from stream");

        ModFile modInfo = new() {
            Id = new Guid(),
            InternalModName = tmodFile.Name,
            ModVersion = tmodFile.Version,
            ModLoaderVersion = tmodFile.ModLoaderVersion
        };
        
        await StoreFile(modFile, modInfo, outputDirectory);
    }

    public async Task<Stream> DownloadModFile(string url) {
        await using Stream stream = await _client.GetStreamAsync(url);
        
        // Copy to a memory stream to ensure it is fully downloaded
        MemoryStream memoryStream = new();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        return memoryStream;
    }
}