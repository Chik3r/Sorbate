using Microsoft.EntityFrameworkCore;

namespace Sorbate.Storage.Models;

public class StorageContext : DbContext {
    public DbSet<ModFile> Files { get; set; }
    
    public string DbPath { get; } = Path.Combine(Directory.GetCurrentDirectory(), "storage.db");

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
}