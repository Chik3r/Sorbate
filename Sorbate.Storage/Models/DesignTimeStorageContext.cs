using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sorbate.Storage.Models;

public class DesignTimeStorageContext : IDesignTimeDbContextFactory<StorageContext> {
    public StorageContext CreateDbContext(string[] args) {
        DbContextOptionsBuilder<StorageContext> optionsBuilder = new();
        optionsBuilder.UseNpgsql("Host=localhost;Database=postgres;Username=postgres;Password=postgres;");
        
        return new StorageContext(optionsBuilder.Options);

    }
}