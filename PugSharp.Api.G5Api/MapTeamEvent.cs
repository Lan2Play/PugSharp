using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class MapTeamEvent : MapEvent
{
    [JsonPropertyName("team_int")]
    public int TeamNumber { get; set; }

    protected MapTeamEvent(string matchId, int mapNumber, int teamNumber, string eventName) : base(matchId, mapNumber, eventName)
    {
        TeamNumber = teamNumber;
    }
}

