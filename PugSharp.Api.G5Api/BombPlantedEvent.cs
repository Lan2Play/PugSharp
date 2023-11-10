namespace PugSharp.Api.G5Api;

public sealed class BombPlantedEvent : PlayerBombEvent
{
    public BombPlantedEvent(string matchId, int mapNumber, int roundNumber, int roundTime, Player player, BombSite site) : base(matchId, mapNumber, roundNumber, roundTime, player, site, "bomb_planted")
    {

    }
}
