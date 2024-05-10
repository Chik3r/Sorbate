using System.ComponentModel.DataAnnotations;

namespace Sorbate.Storage.Models;

public class ModFile {
    public Guid Id { get; set; }
    
    [MaxLength(512)]
    public required string InternalModName { get; set; } = null!;

    [MaxLength(256)]
    public required string ModVersion { get; set; }

    [MaxLength(256)]
    public required string ModLoaderVersion { get; set; }
}