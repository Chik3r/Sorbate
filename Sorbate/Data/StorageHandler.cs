using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Tomat.FNB.TMOD;
using Tomat.FNB.TMOD.Converters.Extractors;
using Tomat.FNB.TMOD.Utilities;

namespace Sorbate.Data;

public class StorageHandler(IDbContextFactory<AppDbContext> dbFactory) : IStorage {
    public async Task<bool> Upload(ModRecord record) {
        if (record.Data is null) {
            // TODO: proper logging
            Console.WriteLine("data is null, uh oh");

            return false;
        }
        SerializableTmodFile modFile = record.Data.Value;
        
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();

        // TODO: do checks on the record, filter if already uploaded etc 
        
        // pretend we uploaded it to the object storage
        var g = Guid.CreateVersion7();

        record.FileName = g.ToString();
        record.FileId = "test_id";

        string? displayName = null;
        string? author = null;
        if (modFile.Entries.TryGetValue("Info", out ISerializableTmodFile.FileEntry info) is true) {
            byte[] data = TmodExtensions.Decompress(info.Data!, info.Length);
            Dictionary<string, string?> convertedInfo = InfoDictExtractor.Convert(data);

            displayName = convertedInfo["displayName"];
            author = convertedInfo["author"];

            if (string.IsNullOrWhiteSpace(displayName))
                displayName = null;
            
            if (string.IsNullOrWhiteSpace(author))
                author = null;
        }
        record.DisplayName = displayName;
        record.Author = author;

        record.Version = modFile.Version;
        record.ModLoaderVersion = modFile.ModLoaderVersion;
        record.InternalName = modFile.Name;

        EntityEntry<ModRecord> entry = await db.AddAsync(record);
        
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UploadRange(IEnumerable<ModRecord> records) {
        foreach (ModRecord record in records) {
            await Upload(record);
        }

        return true;
        // throw new NotImplementedException();
    }

    public Task<string> DownloadLink(int id) {
        throw new NotImplementedException();
    }

    public Task<ModRecord> List(int page, int limit) {
        throw new NotImplementedException();
    }
}