using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Tomat.FNB.TMOD;
using Tomat.FNB.TMOD.Converters;
using Tomat.FNB.TMOD.Converters.Extractors;
using Tomat.FNB.TMOD.Utilities;

namespace Sorbate.Data;

public class StorageHandler(IDbContextFactory<AppDbContext> dbFactory) : IStorage {
    public async Task<bool> Upload(ModRecord record) {
        if (record.Data?.Hash is null) {
            // TODO: proper logging
            Console.WriteLine("Either data or hash is null, uh oh");

            return false;
        }
        SerializableTmodFile modFile = record.Data.Value;
        
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();

        if (db.ModRecords.Any(x => x.Hash == modFile.Hash)) {
            Console.WriteLine($"Mod {modFile.Name} (version {modFile.Version}) already exists, skip.");
            
            // TODO: do more checks on the record, filter if already uploaded etc 
            
            return true;
        }
        

        PopulateModMetadata(record, modFile);

        if (modFile.Entries.TryGetValue("icon_workshop.rawimg", out ISerializableTmodFile.FileEntry rawImageEntry) ||
            modFile.Entries.TryGetValue("icon.rawimg", out rawImageEntry)) {
            IFileConverter extractor = RawimgExtractor.GetRawimgExtractor();
            byte[] rawImage = TmodExtensions.Decompress(rawImageEntry.Data!, rawImageEntry.Length);

            (string path, byte[] data) = extractor.Convert("icon.rawimg", rawImage);
            
            // TODO: Upload mod icon
        }
        
        // TODO: Upload mod
        
        // pretend we uploaded it to the object storage
        var g = Guid.CreateVersion7();

        record.FileName = g.ToString();
        record.FileId = "test_id";

        if (record.PublishedFileId is not null) {
            int id = db.SteamUpdateRecords.FirstOrDefault(x => x.PublishedFileId == record.PublishedFileId)?.Id ?? 0;
            
            SteamUpdateRecord updateRecord = new() {
                Id = id,
                PublishedFileId = record.PublishedFileId,
                TimeUpdated = record.Timestamp,
            };

            db.Update(updateRecord);
        }

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

    public async Task<DateTime?> GetLastUpdateTimestamp(string fileId) {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();

        SteamUpdateRecord? record = await db.SteamUpdateRecords.FirstOrDefaultAsync(x => x.PublishedFileId == fileId);
        return record?.TimeUpdated;
    }

    private static void PopulateModMetadata(ModRecord record, SerializableTmodFile modFile) {
        string? displayName = null;
        string? author = null;
        if (modFile.Entries.TryGetValue("Info", out ISerializableTmodFile.FileEntry info)) {
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
    }
}