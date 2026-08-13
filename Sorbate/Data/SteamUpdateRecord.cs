using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Sorbate.Data;

[Index(nameof(PublishedFileId))]
public record SteamUpdateRecord {
    public int Id { get; set; }
    
    [Required]
    public required string PublishedFileId { get; set; }
    public required DateTime TimeUpdated { get; set; }
}