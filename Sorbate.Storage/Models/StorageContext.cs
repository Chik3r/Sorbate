using Microsoft.EntityFrameworkCore;

namespace Sorbate.Storage.Models;

public sealed class StorageContext : DbContext {
    public DbSet<ModFile> Files { get; set; }

    public StorageContext(DbContextOptions<StorageContext> options) : base(options) {
        // TODO: Regenerate postgres password
    }
}