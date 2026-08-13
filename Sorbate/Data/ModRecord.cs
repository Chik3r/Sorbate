using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tomat.FNB.TMOD;

namespace Sorbate.Data;

public record ModRecord {
    public int Id { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    // Steam, 1.3 mod browser, etc.
    public string? Source { get; set; }
    
    // For example, the published file id from steam
    public string? PublishedFileId { get; set; }
    
    // Hash of the contents of the tmod file
    [Length(20, 20), Required]
    public byte[]? Hash { get; set; }
    
    [Required]
    public string? FileName { get; set; }
    
    // ID of the .tmod and .png files in the object storage
    public string? ModObjectId { get; set; }
    public string? IconObjectId { get; set; }
    
    [Required]
    [DefaultValue(false)]
    public bool Hidden { get; set; }
    
    // Mod data
    [Required]
    public string? InternalName { get; set; }
    
    public string? DisplayName { get; set; }
    
    public string? Author { get; set; }
    
    [Required]
    public string? Version { get; set; }
    
    [Required]
    public string? ModLoaderVersion { get; set; }
    
    [NotMapped]
    public SerializableTmodFile? Data { get; set; }
}