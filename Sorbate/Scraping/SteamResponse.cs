using System.Text.Json.Serialization;

namespace Sorbate.Scraping;

public record SteamResponseRoot(
    [property: JsonPropertyName("response")]
    SteamResponse Response);

public record SteamResponse(
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("publishedfiledetails")]
    IReadOnlyList<PublishedFileDetail> PublishedFileDetails,
    [property: JsonPropertyName("next_cursor")]
    string NextCursor);

public record PublishedFileDetail(
    [property: JsonPropertyName("result")] int Result,
    [property: JsonPropertyName("publishedfileid")]
    string PublishedFileId,
    [property: JsonPropertyName("creator")]
    string Creator,
    [property: JsonPropertyName("creator_appid")]
    int CreatorAppid,
    [property: JsonPropertyName("consumer_appid")]
    int ConsumerAppid,
    [property: JsonPropertyName("consumer_shortcutid")]
    int ConsumerShortcutId,
    [property: JsonPropertyName("filename")]
    string Filename,
    [property: JsonPropertyName("file_size")]
    string FileSize,
    [property: JsonPropertyName("preview_file_size")]
    string PreviewFileSize,
    [property: JsonPropertyName("preview_url")]
    string PreviewUrl,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("hcontent_file")]
    string HContentFile,
    [property: JsonPropertyName("hcontent_preview")]
    string HContentPreview,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("short_description")]
    string ShortDescription,
    [property: JsonPropertyName("time_created")]
    int TimeCreated,
    [property: JsonPropertyName("time_updated")]
    int TimeUpdated,
    [property: JsonPropertyName("visibility")]
    int Visibility,
    [property: JsonPropertyName("flags")] int Flags,
    [property: JsonPropertyName("workshop_file")]
    bool WorkshopFile,
    [property: JsonPropertyName("workshop_accepted")]
    bool WorkshopAccepted,
    [property: JsonPropertyName("show_subscribe_all")]
    bool ShowSubscribeAll,
    [property: JsonPropertyName("num_comments_public")]
    int NumCommentsPublic,
    [property: JsonPropertyName("banned")] bool Banned,
    [property: JsonPropertyName("ban_reason")]
    string BanReason,
    [property: JsonPropertyName("banner")] string Banner,
    [property: JsonPropertyName("can_be_deleted")]
    bool CanBeDeleted,
    [property: JsonPropertyName("app_name")]
    string AppName,
    [property: JsonPropertyName("file_type")]
    int FileType,
    [property: JsonPropertyName("can_subscribe")]
    bool CanSubscribe,
    [property: JsonPropertyName("subscriptions")]
    int Subscriptions,
    [property: JsonPropertyName("favorited")]
    int Favorited,
    [property: JsonPropertyName("followers")]
    int Followers,
    [property: JsonPropertyName("lifetime_subscriptions")]
    int LifetimeSubscriptions,
    [property: JsonPropertyName("lifetime_favorited")]
    int LifetimeFavorited,
    [property: JsonPropertyName("lifetime_followers")]
    int LifetimeFollowers,
    [property: JsonPropertyName("lifetime_playtime")]
    string LifetimePlaytime,
    [property: JsonPropertyName("lifetime_playtime_sessions")]
    string LifetimePlaytimeSessions,
    [property: JsonPropertyName("views")] int Views,
    [property: JsonPropertyName("num_children")]
    int NumChildren,
    [property: JsonPropertyName("num_reports")]
    int NumReports,
    [property: JsonPropertyName("tags")] IReadOnlyList<Tag> Tags,
    [property: JsonPropertyName("language")]
    int Language,
    [property: JsonPropertyName("maybe_inappropriate_sex")]
    bool MaybeInappropriateSex,
    [property: JsonPropertyName("maybe_inappropriate_violence")]
    bool MaybeInappropriateViolence,
    [property: JsonPropertyName("revision_change_number")]
    string RevisionChangeNumber,
    [property: JsonPropertyName("revision")]
    int Revision,
    [property: JsonPropertyName("available_revisions")]
    IReadOnlyList<int> AvailableRevisions,
    [property: JsonPropertyName("ban_text_check_result")]
    int BanTextCheckResult);

public record Tag(
    [property: JsonPropertyName("tag")] string TagName,
    [property: JsonPropertyName("display_name")]
    string DisplayName);