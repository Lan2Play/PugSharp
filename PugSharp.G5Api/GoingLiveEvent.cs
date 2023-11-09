namespace PugSharp.G5Api;

public sealed class GoingLiveEvent : MapEvent
{
    public GoingLiveEvent(string matchId, int mapNumber) : base(matchId, mapNumber, "going_live")
    {
    }
}
