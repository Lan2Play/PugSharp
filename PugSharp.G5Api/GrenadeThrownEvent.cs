namespace PugSharp.G5Api;

public sealed class GrenadeThrownEvent : PlayerWeaponEvent
{
    public GrenadeThrownEvent(string matchId, int mapNumber, int roundNumber, int roundTime, Player player, Weapon weapon) : base(matchId, mapNumber, roundNumber, roundTime, player, weapon, "grenade_thrown")
    {
    }
}
