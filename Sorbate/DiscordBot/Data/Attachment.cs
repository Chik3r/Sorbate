using System.Text.Json.Serialization;

namespace Sorbate.DiscordBot.Data;

public record Attachment(
    [property: JsonPropertyName("id")]
    string Id,
    [property: JsonPropertyName("filename")]
    string Filename,
    [property: JsonPropertyName("url")]
    string Url
);