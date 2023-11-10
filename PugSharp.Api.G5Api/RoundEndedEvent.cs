using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class RoundEndedEvent : TimedRoundEvent
{
    // Note that reason is decremented by 1 to match the values defined at https://github.com/alliedmodders/sourcemod/blob/master/plugins/include/cstrike.inc
    // CSGO increments these by 1 for some reason.

    [JsonPropertyName("reason")]
    public int Reason { get; set; }

    [JsonPropertyName("winner")]
    public Winner Winner { get; set; }

    [JsonPropertyName("team1")]
    public StatsTeam StatsTeam1 { get; set; }

    [JsonPropertyName("team2")]
    public StatsTeam StatsTeam2 { get; set; }

    public RoundEndedEvent(string matchId, int mapNumber, int roundNumber, int roundTime, Winner winner, StatsTeam statsTeam1, StatsTeam statsTeam2, int reason) : base(matchId, mapNumber, roundNumber, roundTime, "round_end")
    {
        Reason = reason;
        Winner = winner;
        StatsTeam1 = statsTeam1;
        StatsTeam2 = statsTeam2;
    }
}
