using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class GrenadeVictim
{

    [JsonPropertyName("player")]
    public Player Player { get; set; }

    [JsonPropertyName("friendly_fire")]
    public bool FriendlyFire { get; set; }

    protected GrenadeVictim(Player player, bool friendlyFire)
    {
        Player = player;
        FriendlyFire = friendlyFire;
    }

}
