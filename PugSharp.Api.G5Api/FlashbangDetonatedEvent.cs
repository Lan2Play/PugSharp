namespace PugSharp.Api.G5Api;

public sealed class FlashbangDetonatedEvent : VictimWithDamageGrenadeEvent
{
    public FlashbangDetonatedEvent(string matchId, int mapNumber, int roundNumber, int roundTime, Player player, IEnumerable<GrenadeVictim> victims, int damageEnemies, int damageFriendlies) : base(matchId, mapNumber, roundNumber, roundTime, player, new Weapon("flashbang", CsWeaponId.FLASHBANG), victims, damageEnemies, damageFriendlies, "flashbang_detonated")
    {
    }
}
