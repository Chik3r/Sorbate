using System.Text.Json.Serialization;

namespace Sorbate.FileScrapers.Discord.Data;

public record SearchResults(
    [property: JsonPropertyName("analytics_id")]
    string AnalyticsId,
    [property: JsonPropertyName("doing_deep_historical_index")]
    bool DoingDeepHistoricalIndex,
    [property: JsonPropertyName("messages")]
    Message[][] Messages,
    [property: JsonPropertyName("total_results")]
    int TotalResults
);