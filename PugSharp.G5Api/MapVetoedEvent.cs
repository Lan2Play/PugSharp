namespace PugSharp.G5Api;

public sealed class MapVetoedEvent : MapSelectionEvent
{
    public MapVetoedEvent(string matchId, int teamNumber, string mapName) : base(matchId, teamNumber, mapName, "map_vetoed")
    {
    }
}
