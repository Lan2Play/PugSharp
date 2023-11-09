using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public class VictimWithDamageGrenadeEvent : VictimGrenadeEvent
{
    [JsonPropertyName("damage_enemies")]
    public int DamageEnemies { get; set; }

    [JsonPropertyName("damage_friendlies")]
    public int DamageFriendlies { get; set; }

    protected VictimWithDamageGrenadeEvent(string matchId, int mapNumber, int roundNumber, int roundTime, Player player, Weapon weapon, IEnumerable<GrenadeVictim> victims, int damageEnemies, int damageFriendlies, string eventName) : base(matchId, mapNumber, roundNumber, roundTime, player, weapon, victims, eventName)
    {
        DamageEnemies = damageEnemies;
        DamageFriendlies = damageFriendlies;
    }

}
