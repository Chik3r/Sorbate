namespace Sorbate.Data;

public record SorbateApiListing {
    public required List<SorbateApiRecord> Records { get; set; }
    public required int Total { get; set; }
}