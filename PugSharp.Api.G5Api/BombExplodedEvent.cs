namespace PugSharp.Api.G5Api;

public sealed class BombExplodedEvent : BombEvent
{
    public BombExplodedEvent(string matchId, int mapNumber, int roundNumber, int roundTime, BombSite site) : base(matchId, mapNumber, roundNumber, roundTime, site, "bomb_exploded")
    {
    }
}
