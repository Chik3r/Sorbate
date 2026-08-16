namespace Sorbate.Data;

public interface IStorage {
    public Task<bool> Upload(ModRecord record);
    
    public Task<bool> UploadRange(IAsyncEnumerable<ModRecord> records);
    
    public Task<string?> GetModDownloadLink(int id);

    public Task<string?> GetIconDownloadLink(int id);
    
    public Task<IList<ModRecord>> ListMods(int page, int limit, string? name = null, string? author = null, string? version = null);

    public Task<DateTime?> GetLastUpdateTimestamp(string fileId);

    // This could return all of the info file data
    // public ModRecord Info(int id);
}