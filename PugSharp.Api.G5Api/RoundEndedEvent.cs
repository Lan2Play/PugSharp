using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class RoundEndedEvent : TimedRoundEvent
{
    // Note that reason is decremented by 1 to match the values defined at https://github.com/alliedmodders/sourcemod/blob/master/plugins/include/cstrike.inc
    // CSGO increments these by 1 for some reason.

    [JsonPropertyName("reason")]
    public required int Reason { get; init; }

    [JsonPropertyName("winner")]
    public required Winner Winner { get; init; }

    [JsonPropertyName("team1")]
    public required StatsTeam StatsTeam1 { get; init; }

    [JsonPropertyName("team2")]
    public required StatsTeam StatsTeam2 { get; init; }

    public RoundEndedEvent() : base("round_end")
    {
    }
}
