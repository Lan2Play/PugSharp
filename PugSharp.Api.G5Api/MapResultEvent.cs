using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class MapResultEvent : MapEvent
{
    [JsonPropertyName("winner")]
    public required Winner Winner { get; init; }

    [JsonPropertyName("team1")]
    public required StatsTeam StatsTeam1 { get; init; }

    [JsonPropertyName("team2")]
    public required StatsTeam StatsTeam2 { get; init; }

    public MapResultEvent() : base("map_result")
    {
    }
}
