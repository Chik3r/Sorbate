using System.Text.Json.Serialization;

namespace Sorbate.FileScrapers.Discord.Data.Events;

public record EventHello(
    [property: JsonPropertyName("heartbeat_interval")]
    int HeartbeatInterval
);