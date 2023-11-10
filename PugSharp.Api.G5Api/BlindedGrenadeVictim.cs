using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class BlindedGrenadeVictim : GrenadeVictim
{
    [JsonPropertyName("blind_duration")]
    public float BlindDuration { get; set; }

    public BlindedGrenadeVictim(float blindDuration, Player player, bool friendlyFire) : base(player, friendlyFire)
    {
        BlindDuration = blindDuration;
    }
}
