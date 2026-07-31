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
    
    public bool Hidden { get; set; }
    
    [NotMapped]
    public SerializableTmodFile? Data { get; set; }
}