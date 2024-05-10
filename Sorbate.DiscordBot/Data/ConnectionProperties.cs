using System.Text.Json.Serialization;

namespace Sorbate.DiscordBot.Data;

public record ConnectionProperties(
    [property: JsonPropertyName("os")]
    string OperatingSystem,
    [property: JsonPropertyName("browser")]
    string Browser,
    [property: JsonPropertyName("device")]
    string Device
) {
    public static ConnectionProperties Default => new("linux", "disco", "disco");
}
