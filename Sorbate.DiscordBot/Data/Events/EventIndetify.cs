using System.Text.Json.Serialization;

namespace Sorbate.DiscordBot.Data.Events;

public record EventIdentify(
    [property: JsonPropertyName("token")]
    string AuthToken,
    [property: JsonPropertyName("properties")]
    ConnectionProperties ConnectionProperties,
    [property: JsonPropertyName("intents")]
    int GatewayIntents,
    [property: JsonPropertyName("compress")]
    bool PacketCompressionSupported = false,
    [property: JsonPropertyName("large_threshold")]
    int GuildLargeThreshold = 50
);