using Sorbate.Storage.Models;
using Tomat.FNB.TMOD;

namespace Sorbate.Storage;

public class StorageHandler {
    public static StorageHandler Instance { get; } = new();
    
    public StorageContext Db { get; } = new();
    
    private StorageHandler() { }

    public async Task StoreFile(Stream fileData, ModFile fileInfo, string outputDirectory) {
        Directory.CreateDirectory(outputDirectory);

        Guid fileGuid = Guid.NewGuid();
        fileInfo.Id = fileGuid;
        string fileName = Path.ChangeExtension(fileGuid.ToString(), ".tmod");
        string filePath = Path.Combine(outputDirectory, fileName);

        await using FileStream fileStream = File.Create(filePath);
        await fileData.CopyToAsync(fileStream);

        Db.Files.Add(fileInfo);
        await Db.SaveChangesAsync();
    }

    public async Task StoreTmodFile(Stream modFile, string outputDirectory) {
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
}