using System.Text.Json.Serialization;

namespace Sorbate.DiscordBot.Data.Events;

public record EventMessageCreate(
    string Id,
    string ChannelId,
    DateTime Timestamp,
    Attachment[] Attachments,
    [property: JsonPropertyName("guild_id")]
    string? GuildId
) : Message(Id, ChannelId, Timestamp, Attachments);