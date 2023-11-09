using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public sealed class SeriesStartedEvent : MatchEvent
{
    [JsonPropertyName("num_maps")]
    public int NumberOfMaps { get; set; }

    [JsonPropertyName("team1")]
    public TeamWrapper Team1 { get; set; }

    [JsonPropertyName("team2")]
    public TeamWrapper Team2 { get; set; }

    public SeriesStartedEvent(string matchId, TeamWrapper team1, TeamWrapper team2, int numberOfMaps) : base(matchId, "series_start")
    {
        Team1 = team1;
        Team2 = team2;
        NumberOfMaps = numberOfMaps;
    }
}
