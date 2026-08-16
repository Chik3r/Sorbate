namespace Sorbate.Data;

public record SorbateApiRecord {
    public required int Id { get; set; }
    public string? IconUrl { get; set; }
    public string? DisplayName { get; set; }
    public required string Author { get; set; }
    public required string InternalName { get; set; }
    public required string Version { get; set; }
    public required string ModLoaderVersion { get; set; }
    public required DateTime Timestamp { get; set; }
}