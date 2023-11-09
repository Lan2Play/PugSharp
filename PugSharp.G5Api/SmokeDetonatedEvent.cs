using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public sealed class SmokeDetonatedEvent : PlayerWeaponEvent
{
    [JsonPropertyName("extinguished_molotov")]
    public bool ExtinguishedMolotov { get; set; }

    public SmokeDetonatedEvent(string matchId, int mapNumber, int roundNumber, int roundTime, Player player, bool extinguishedMolotov) : base(matchId, mapNumber, roundNumber, roundTime, player, new Weapon("smokegrenade", CsWeaponId.SMOKEGRENADE), "smokegrenade_detonated")
    {
        ExtinguishedMolotov = extinguishedMolotov;
    }
}
