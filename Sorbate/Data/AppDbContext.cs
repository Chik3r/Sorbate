using Microsoft.EntityFrameworkCore;

namespace Sorbate.Data;

public class AppDbContext : DbContext {
    public DbSet<ModRecord> ModRecords => Set<ModRecord>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        // TODO: Enable and test
        modelBuilder.Entity<ModRecord>()
            .HasIndex(u => u.Hash)
            .IsUnique();
    }
}