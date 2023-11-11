using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class VictimWithDamageGrenadeEvent : VictimGrenadeEvent
{
    [JsonPropertyName("damage_enemies")]
    public required int DamageEnemies { get; init; }

    [JsonPropertyName("damage_friendlies")]
    public required int DamageFriendlies { get; init; }

    protected VictimWithDamageGrenadeEvent(Weapon weapon, string eventName) : base(weapon, eventName)
    {
    }

}
