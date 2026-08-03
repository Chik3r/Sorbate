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

        // Check if we already have a file with the same hash.
        // If so, then do not upload this file, and make sure to write down when we last updated the related published file id.
        if (await db.ModRecords.AnyAsync(x => x.Hash == modFile.Hash)) {
            Console.WriteLine($"Mod {modFile.Name} (version {modFile.Version}) already exists, skip.");
            
            // TODO: do more checks on the record, filter if already uploaded etc 

            await UpdateRecordTimestamp(record, db);
            await db.SaveChangesAsync();
            
            return true;
        }
        
        // Read mod metadata (such as author, display name, etc.) from the .tmod file, 
        // and write it to the ModRecord.
        PopulateModMetadata(record, modFile);

        // Upload the mod and its icon, storing the file as <guid>.<extension>
        var g = Guid.CreateVersion7();
        UploadIcon(modFile, g); // Gets the mod icon from the .tmod file, and uploads it
        UploadMod(record, g);

        await UpdateRecordTimestamp(record, db);
        EntityEntry<ModRecord> entry = await db.AddAsync(record);
        
        await db.SaveChangesAsync();
        return true;
    }

    private static void UploadMod(ModRecord record, Guid g) {
        // TODO: Upload mod
        
        // pretend we uploaded it to the object storage
        

        record.FileName = g.ToString();
        record.FileId = "test_id";
    }

    public async Task<bool> UploadRange(IAsyncEnumerable<ModRecord> records) {
        await foreach (ModRecord record in records) {
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
    
    private static async Task UpdateRecordTimestamp(ModRecord record, AppDbContext db) {
        if (record.PublishedFileId is not null) {
            SteamUpdateRecord? updateRecord = await db.SteamUpdateRecords
                .FirstOrDefaultAsync(x => x.PublishedFileId == record.PublishedFileId);

            updateRecord ??= new SteamUpdateRecord {
                PublishedFileId = record.PublishedFileId,
                TimeUpdated = record.Timestamp,
            };

            updateRecord.TimeUpdated = record.Timestamp;

            db.Update(updateRecord);
        }
    }
    
    private static void UploadIcon(SerializableTmodFile modFile, Guid g) {
        if (modFile.Entries.TryGetValue("icon_workshop.rawimg", out ISerializableTmodFile.FileEntry rawImageEntry) ||
            modFile.Entries.TryGetValue("icon.rawimg", out rawImageEntry)) {
            IFileConverter extractor = RawimgExtractor.GetRawimgExtractor();
            byte[] rawImage = TmodExtensions.Decompress(rawImageEntry.Data!, rawImageEntry.Length);

            (string path, byte[] data) = extractor.Convert("icon.rawimg", rawImage);
            
            // TODO: Upload mod icon
        }
    }
}