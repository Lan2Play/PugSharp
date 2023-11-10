using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class MatchTeamEvent : MatchEvent
{
    [JsonPropertyName("team_int")]
    public int TeamNumber { get; set; }

    protected MatchTeamEvent(string matchId, int teamNumber, string eventName) : base(matchId, eventName)
    {
        TeamNumber = teamNumber;
    }
}
