using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Sorbate.Data;

public class StorageHandler(IDbContextFactory<AppDbContext> dbFactory) : IStorage {
    public async Task<bool> Upload(ModRecord record) {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();

        // TODO: do checks on the record, filter if already uploaded etc 
        
        // pretend we uploaded it to the object storage
        var g = Guid.CreateVersion7();

        record.FileName = g.ToString();
        record.FileId = "test_id";

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