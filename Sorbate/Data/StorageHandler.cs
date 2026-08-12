using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Tomat.FNB.TMOD;
using Tomat.FNB.TMOD.Converters;
using Tomat.FNB.TMOD.Converters.Extractors;
using Tomat.FNB.TMOD.Utilities;

namespace Sorbate.Data;

public class StorageHandler : IStorage {
    private readonly AmazonS3Client _s3Store;
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly string _bucketName;
    
    private static readonly SemaphoreSlim DbSemaphore = new(1, 1);

    public StorageHandler(IDbContextFactory<AppDbContext> dbFactory, IConfiguration configuration) {
        _dbFactory = dbFactory;

        string? accessKey = configuration.GetValue<string>("S3Storage:AccessKey");
        string? secretKey = configuration.GetValue<string>("S3Storage:SecretKey");
        string? bucketName = configuration.GetValue<string>("S3Storage:BucketName");
        string? serviceUrl = configuration.GetValue<string>("S3Storage:ServiceUrl");
        string? region = configuration.GetValue<string>("S3Storage:Region");

        if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey) ||
            string.IsNullOrWhiteSpace(bucketName) || string.IsNullOrWhiteSpace(serviceUrl) ||
            string.IsNullOrWhiteSpace(region)) {
            throw new ArgumentException("Failed to initialize storage handler, missing values.");
        }

        _bucketName = bucketName;

        AWSConfigsS3.UseSignatureVersion4 = false;
        _s3Store = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config() {
            AuthenticationRegion = region,
            ServiceURL = serviceUrl,
            ForcePathStyle = true,
        });
    }

    public async Task<bool> Upload(ModRecord record) {
        if (record.Data?.Hash is null) {
            // TODO: proper logging
            Console.WriteLine("Either data or hash is null, uh oh");

            return false;
        }

        try {
            await DbSemaphore.WaitAsync();
            SerializableTmodFile modFile = record.Data.Value;

            await using AppDbContext db = await _dbFactory.CreateDbContextAsync();

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
            record.FileName = g.ToString();
            await UploadIcon(record, g); // Gets the mod icon from the .tmod file, and uploads it
            await UploadMod(record, g);

            await UpdateRecordTimestamp(record, db);
            EntityEntry<ModRecord> entry = await db.AddAsync(record);

            await db.SaveChangesAsync();
            return true;
        }
        finally {
            DbSemaphore.Release();
        }
    }

    public async Task<bool> UploadRange(IAsyncEnumerable<ModRecord> records) {
        await foreach (ModRecord record in records) {
            await Upload(record);
        }

        return true;
    }

    public async Task<string?> GetModDownloadLink(int id) {
        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();
        ModRecord? record = await db.ModRecords.FindAsync(id);

        if (record?.ModObjectId is null) return null;

        return await GetPresignedUrl(record.ModObjectId);
    }

    public async Task<string?> GetIconDownloadLink(int id) {
        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();
        ModRecord? record = await db.ModRecords.FindAsync(id);

        if (record?.IconObjectId is null) return null;
        
        return await GetPresignedUrl(record.IconObjectId);
    }

    public async Task<IList<ModRecord>> ListMods(int page, int limit) {
        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();
        limit = Math.Min(limit, 100);
        
        List<ModRecord> records = await db.ModRecords
            .OrderByDescending(x => x.Id)
            .Skip(limit * page)
            .Take(limit)
            .ToListAsync();
        return records;
    }

    public async Task<DateTime?> GetLastUpdateTimestamp(string fileId) {
        await using AppDbContext db = await _dbFactory.CreateDbContextAsync();

        SteamUpdateRecord? record = await db.SteamUpdateRecords.FirstOrDefaultAsync(x => x.PublishedFileId == fileId);
        return record?.TimeUpdated;
    }

    private static void PopulateModMetadata(ModRecord record, SerializableTmodFile modFile) {
        string? displayName = null;
        string? author = null;
        if (modFile.Entries.TryGetValue("Info", out ISerializableTmodFile.FileEntry info)) {
            byte[] data = TmodExtensions.Decompress(info.Data!, info.Length);
            Dictionary<string, string?> convertedInfo = InfoDictExtractor.Convert(data);

            displayName = convertedInfo.GetValueOrDefault("displayName");
            author = convertedInfo.GetValueOrDefault("author");

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
    
    private async Task UploadIcon(ModRecord record, Guid g) {
        SerializableTmodFile modFile = record.Data!.Value;
        
        byte[]? icon = null;
        if (modFile.Entries.TryGetValue("icon.png", out ISerializableTmodFile.FileEntry iconEntry)) {
            icon = TmodExtensions.Decompress(iconEntry.Data!, iconEntry.Length);
        }
        else if (modFile.Entries.TryGetValue("icon.rawimg", out ISerializableTmodFile.FileEntry rawIconEntry)) {
            IFileConverter extractor = RawimgExtractor.GetRawimgExtractor();
            byte[] rawImage = TmodExtensions.Decompress(rawIconEntry.Data!, rawIconEntry.Length);

            (string _, byte[] data) = extractor.Convert("icon.rawimg", rawImage);
            icon = data;
        }
        else if (modFile.Entries.TryGetValue("icon_workshop.png", out iconEntry)) {
            icon = TmodExtensions.Decompress(iconEntry.Data!, iconEntry.Length);
        }
        else if (modFile.Entries.TryGetValue("icon_workshop.rawimg", out rawIconEntry)) {
            IFileConverter extractor = RawimgExtractor.GetRawimgExtractor();
            byte[] rawImage = TmodExtensions.Decompress(rawIconEntry.Data!, rawIconEntry.Length);

            (string _, byte[] data) = extractor.Convert("icon.rawimg", rawImage);
            icon = data;
        }
        else {
            Console.WriteLine($"Icon not found for mod {record.InternalName}-v{record.Version}");
            return;
        }
        
        // Upload the mod icon
        await using MemoryStream ms = new();
        await ms.WriteAsync(icon);
        
        string fileName = Path.ChangeExtension(g.ToString(), ".png");
        await UploadObject(ms, fileName);
        record.IconObjectId = fileName;
    }
    
    private async Task UploadMod(ModRecord record, Guid g) {
        SerializableTmodFile modFile = record.Data!.Value;
        
        // Write the mod back into the .tmod format in memory
        await using MemoryStream ms = new();
        modFile.Write(ms, true);
        
        // Upload the .tmod
        string fileName = Path.ChangeExtension(g.ToString(), ".tmod");
        await UploadObject(ms, fileName);
        record.ModObjectId = fileName;
    }

    private async Task UploadObject(Stream stream, string key) {
        TransferUtility utility = new(_s3Store);
        await utility.UploadAsync(stream, _bucketName, key);
    }

    private async Task<string?> GetPresignedUrl(string fileName) {
        try {
            GetPreSignedUrlRequest request = new() {
                BucketName = _bucketName,
                Key = fileName,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddHours(1)
            };
            return await _s3Store.GetPreSignedURLAsync(request);
        }
        catch {
            return null;
        }
    }
}