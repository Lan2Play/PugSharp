namespace PugSharp.G5Api;

public sealed class HeDetonatedEvent : VictimWithDamageGrenadeEvent
{
    public HeDetonatedEvent(string matchId, int mapNumber, int roundNumber, int roundTime, Player player, IEnumerable<GrenadeVictim> victims, int damageEnemies, int damageFriendlies) : base(matchId, mapNumber, roundNumber, roundTime, player, new Weapon("hegrenade", CsWeaponId.HEGRENADE), victims, damageEnemies, damageFriendlies, "hegrenade_detonated")
    {
    }
}
