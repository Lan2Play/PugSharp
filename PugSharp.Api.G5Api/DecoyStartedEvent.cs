namespace PugSharp.Api.G5Api;

public sealed class DecoyStartedEvent : PlayerWeaponEvent
{
    public DecoyStartedEvent(string matchId, int mapNumber, int roundNumber, int roundTime, Player player) : base(matchId, mapNumber, roundNumber, roundTime, player, new Weapon("decoy", CsWeaponId.DECOY), "decoygrenade_started")
    {
    }
}
