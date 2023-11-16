
using System.Text.Json.Serialization;
using PugSharp.Shared;

namespace PugSharp.Api.G5Api;

public sealed class SmokeDetonatedEvent : PlayerWeaponEvent
{
    [JsonPropertyName("extinguished_molotov")]
    public required bool ExtinguishedMolotov { get; init; }

    public SmokeDetonatedEvent() : base(new Weapon("smokegrenade", CSWeaponId.SmokeGrenade), "smokegrenade_detonated")
    {
    }
}