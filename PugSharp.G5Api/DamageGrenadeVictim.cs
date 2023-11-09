using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public sealed class DamageGrenadeVictim : GrenadeVictim
{
    [JsonPropertyName("damage")]
    public int Damage { get; set; }

    [JsonPropertyName("killed")]
    public bool Killed { get; set; }

    public DamageGrenadeVictim(Player player, bool friendlyFire, int damage, bool killed) : base(player, friendlyFire)
    {
        Damage = damage;
        Killed = killed;
    }
}
