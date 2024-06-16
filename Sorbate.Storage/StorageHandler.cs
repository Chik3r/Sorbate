using Sorbate.Storage.Models;
using Tomat.FNB.TMOD;

namespace Sorbate.Storage;

// TODO
//  - Store the SHA1 hash of the tmod file, prevents repeated files by checking if they are already stored

public static class StorageHandler {
    public static async Task StoreFile(Stream fileData, ModFile fileInfo, string outputDirectory) {
        Directory.CreateDirectory(outputDirectory);

        Guid fileGuid = Guid.NewGuid();
        fileInfo.Id = fileGuid;
        string fileName = Path.ChangeExtension(fileGuid.ToString(), ".tmod");
        string filePath = Path.Combine(outputDirectory, fileName);

        await using FileStream fileStream = File.Create(filePath);
        await fileData.CopyToAsync(fileStream);

        await using StorageContext db = new();
        
        db.Files.Add(fileInfo);
        await db.SaveChangesAsync();
    }

    public static async Task StoreTmodFile(Stream modFile, string outputDirectory) {
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