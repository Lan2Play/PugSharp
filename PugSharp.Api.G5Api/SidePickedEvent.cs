using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class SidePickedEvent : MapSelectionEvent
{
    [JsonPropertyName("map_number")]
    public int MapNumber { get; set; }

    [JsonPropertyName("side_int")]
    public int Side { get; set; }

    public SidePickedEvent(string matchId, int teamNumber, string mapName, int mapNumber, Side side) : base(matchId, teamNumber, mapName, "side_picked")
    {
        MapNumber = mapNumber;
        Side = (int)side;
    }
}
