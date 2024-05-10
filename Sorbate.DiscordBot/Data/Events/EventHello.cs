using System.Text.Json.Serialization;

namespace Sorbate.DiscordBot.Data.Events;

public record EventHello(
    [property: JsonPropertyName("heartbeat_interval")]
    int HeartbeatInterval
);