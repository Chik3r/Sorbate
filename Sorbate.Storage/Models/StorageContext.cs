using Microsoft.EntityFrameworkCore;

namespace Sorbate.Storage.Models;

internal sealed class StorageContext : DbContext {
    public DbSet<ModFile> Files { get; set; }
    
    public string ConnectionString { get; }
    
    public StorageContext() {
        // TODO: Read password from environment/configuration file
        // TODO: Regenerate postgres password
        ConnectionString = "Host=localhost;Database=postgres;Username=postgres;Password=L6NFJZ#KmwUkFm";
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseNpgsql(ConnectionString);
}