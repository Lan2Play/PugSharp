using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public class MapEvent : MatchEvent
{
    [JsonPropertyName("map_number")]
    public int MapNumber { get; set; }

    protected MapEvent(string matchId, int mapNumber, string eventName) : base(matchId, eventName)
    {
        MapNumber = mapNumber;
    }
}
