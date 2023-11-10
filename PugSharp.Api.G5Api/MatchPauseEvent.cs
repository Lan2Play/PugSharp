using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class MatchPauseEvent : MapTeamEvent
{
    [JsonPropertyName("pause_type_int")]
    public int PauseType { get; set; }

    protected MatchPauseEvent(string matchId, int mapNumber, int teamNumber, PauseType pauseType, string eventName) : base(matchId, mapNumber, teamNumber, eventName)
    {
        PauseType = (int)pauseType;
    }
}
