using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sorbate.Data;

public class DesignTimeStorageContext : IDesignTimeDbContextFactory<AppDbContext> {
    public AppDbContext CreateDbContext(string[] args) {
        DbContextOptionsBuilder<AppDbContext> optionsBuilder = new();
        optionsBuilder.UseNpgsql("Host=localhost;Database=postgres;Username=postgres;Password=postgres;");
        
        return new AppDbContext(optionsBuilder.Options);

    }
}