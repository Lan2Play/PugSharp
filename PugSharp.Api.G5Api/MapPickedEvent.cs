using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class MapPickedEvent : MapSelectionEvent
{
    [JsonPropertyName("map_number")]
    public int MapNumber { get; set; }

    public MapPickedEvent(string matchId, int teamNumber, int mapNumber, string mapName) : base(matchId, teamNumber, mapName, "map_picked")
    {
        MapNumber = mapNumber;
    }
}
