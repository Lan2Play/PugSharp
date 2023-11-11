using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

// This event fires when the molotov ends, but its RoundTime parameter is when it started burning.
// Note that this event does *not* fire if the molotov was thrown directly at a smoke and did not start burning.
public sealed class MolotovDetonatedEvent : VictimWithDamageGrenadeEvent
{
    [JsonPropertyName("round_time_ended")]
    public required int EndTime { get; init; }

    [JsonPropertyName("duration")]
    public required int Duration { get; init; }

    public MolotovDetonatedEvent() : base(new Weapon("molotov", CsWeaponId.MOLOTOV), "molotov_detonated")
    {
    }
}
