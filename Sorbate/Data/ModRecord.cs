using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tomat.FNB.TMOD;

namespace Sorbate.Data;

public record ModRecord {
    public int Id { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    public string? Source { get; set; }
    
    [Length(20, 20), Required]
    public byte[]? Hash { get; set; }
    
    [Required]
    public string? FileName { get; set; }
    
    public string? FileId { get; set; } // For object storage
    
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