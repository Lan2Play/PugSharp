using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class MapSelectionEvent : MatchTeamEvent
{
    [JsonPropertyName("map_name")]
    public string MapName { get; set; }

    protected MapSelectionEvent(string matchId, int teamNumber, string mapName, string eventName) : base(matchId, teamNumber, eventName)
    {
        MapName = mapName;
    }
}
