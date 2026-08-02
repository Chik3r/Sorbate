namespace Sorbate.Data;

public interface IStorage {
    public Task<bool> Upload(ModRecord record);
    
    public Task<bool> UploadRange(IEnumerable<ModRecord> records);
    
    public Task<string> DownloadLink(int id);
    public Task<ModRecord> List(int page, int limit);
    
    // This could return all of the info file data
    // public ModRecord Info(int id);
}