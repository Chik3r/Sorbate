using Microsoft.EntityFrameworkCore;

namespace Sorbate.Storage.Models;

internal sealed class StorageContext : DbContext {
    public DbSet<ModFile> Files { get; set; }
    
    public string DbPath { get; }
    
    public StorageContext() {
        DbPath = "storage.db";
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
}