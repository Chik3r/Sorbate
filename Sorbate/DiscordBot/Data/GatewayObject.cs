using System.Text.Json.Serialization;

namespace Sorbate.DiscordBot.Data;

public record GatewayObject<T>(
    [property: JsonPropertyName("op")]
    GatewayOpCode OpCode,
    [property: JsonPropertyName("d")]
    T? Data,
    [property: JsonPropertyName("s")]
    int? SequenceNumber = null,
    [property: JsonPropertyName("t")]
    string? EventName = null
);

