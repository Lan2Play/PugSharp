using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class MapResultEvent : MapEvent
{
    [JsonPropertyName("winner")]
    public Winner Winner { get; set; }

    [JsonPropertyName("team1")]
    public StatsTeam StatsTeam1 { get; set; }

    [JsonPropertyName("team2")]
    public StatsTeam StatsTeam2 { get; set; }

    public MapResultEvent(string matchId, int mapNumber, Winner winner, StatsTeam statsTeam1, StatsTeam statsTeam2) : base(matchId, mapNumber, "map_result")
    {
        Winner = winner;
        StatsTeam1 = statsTeam1;
        StatsTeam2 = statsTeam2;
    }
}
