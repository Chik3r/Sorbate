using System.ComponentModel.DataAnnotations;

namespace Sorbate.Storage.Models;

public class ModFile {
    public Guid Id { get; set; }
    
    [MaxLength(256)]
    [Required]
    public string InternalModName { get; set; }
    
    [MaxLength(256)]
    public string ModVersion { get; set; }
}