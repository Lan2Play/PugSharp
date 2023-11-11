using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class SeriesResultEvent : MatchEvent
{
    [JsonPropertyName("time_until_restore")]
    public required int TimeUntilRestore { get; init; }

    [JsonPropertyName("winner")]
    public required Winner Winner { get; init; }

    [JsonPropertyName("team1_series_score")]
    public required int Team1SeriesScore { get; init; }

    [JsonPropertyName("team2_series_score")]
    public required int Team2SeriesScore { get; init; }

    public SeriesResultEvent() : base( "series_end")
    {
    }
}
