using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Sorbate.Storage.Models;

[Index(nameof(FileHash), IsUnique = true)]
public class ModFile {
    public Guid Id { get; set; }
    
    [MaxLength(512)]
    public required string InternalModName { get; set; } = null!;

    [MaxLength(256)]
    public required string ModVersion { get; set; }

    [MaxLength(256)]
    public required string ModLoaderVersion { get; set; }

    /// <summary>
    /// SHA-1 hash of the file. Used to prevent duplicate files from being stored.
    /// </summary>
    [Length(20, 20)]
    [Required] // Required for the database however added after object creation
    public byte[] FileHash { get; set; } = null!;
    
    [MaxLength(512)]
    public string? Author { get; set; }
}