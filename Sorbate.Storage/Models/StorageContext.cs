using Microsoft.EntityFrameworkCore;

namespace Sorbate.Storage.Models;

public class StorageContext : DbContext {
    public DbSet<ModFile> Files { get; set; }
    
    public string DbPath { get; }
    
    public StorageContext() {
        string folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        DbPath = Path.Combine(folder, "storage.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
}