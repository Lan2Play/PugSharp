using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class PlayerWeaponEvent : PlayerTimedRoundEvent
{
    [JsonPropertyName("weapon")]
    public Weapon Weapon { get; }

    protected PlayerWeaponEvent(Weapon weapon, string eventName) : base(eventName)
    {
        Weapon = weapon;
    }
}
