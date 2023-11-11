using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class VictimGrenadeEvent : PlayerWeaponEvent
{
    [JsonPropertyName("victims")]
    public required IEnumerable<GrenadeVictim> Victims { get; init; } = new List<GrenadeVictim>();

    protected VictimGrenadeEvent(Weapon weapon, string eventName) : base(weapon, eventName)
    {
    }
}
