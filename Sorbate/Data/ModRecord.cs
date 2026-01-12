using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sorbate.Data;

public record ModRecord {
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Source { get; set; }
    [Length(20, 20), Required]
    public byte[]? Hash { get; set; }
    [Required]
    public string? FileName { get; set; }
    [NotMapped]
    public byte[]? Data { get; set; }
}