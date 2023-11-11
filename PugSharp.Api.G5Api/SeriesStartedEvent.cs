using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class SeriesStartedEvent : MatchEvent
{
    [JsonPropertyName("num_maps")]
    public required int NumberOfMaps { get; init; }

    [JsonPropertyName("team1")]
    public required TeamWrapper Team1 { get; init; }

    [JsonPropertyName("team2")]
    public required TeamWrapper Team2 { get; init; }

    public SeriesStartedEvent() : base("series_start")
    {
    }
}
