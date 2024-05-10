using System.Text.Json.Serialization;

namespace Sorbate.DiscordBot.Data;

public record Message(
    [property: JsonPropertyName("id")]
    string Id,
    [property: JsonPropertyName("channel_id")]
    string ChannelId,
    [property: JsonPropertyName("timestamp")]
    DateTime Timestamp,
    [property: JsonPropertyName("attachments")]
    Attachment[] Attachments
    // Has many other fields however they are useless for us
);