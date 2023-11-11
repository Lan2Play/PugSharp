using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class PlayerDeathEvent : PlayerWeaponEvent
{
    [JsonPropertyName("bomb")]
    public required bool Bomb { get; init; }

    [JsonPropertyName("headshot")]
    public required bool Headshot { get; init; }

    [JsonPropertyName("thru_smoke")]
    public required bool ThruSmoke { get; init; }

    [JsonPropertyName("penetrated")]
    public required int Penetrated { get; init; }

    [JsonPropertyName("attacker_blind")]
    public required bool AttackerBlind { get; init; }

    [JsonPropertyName("no_scope")]
    public required bool NoScope { get; init; }

    [JsonPropertyName("suicide")]
    public required bool Suicide { get; init; }

    [JsonPropertyName("friendly_fire")]
    public required bool FriendlyFire { get; init; }

    [JsonPropertyName("attacker")]
    public required Player Attacker { get; init; }

    [JsonPropertyName("assist")]
    public required Assister Assist { get; init; }

    public PlayerDeathEvent(Weapon weapon) : base(weapon, "player_death")
    {
    }
}
