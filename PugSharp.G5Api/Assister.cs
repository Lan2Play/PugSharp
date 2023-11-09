using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public class Assister  {
    
    [JsonPropertyName("player")]
    public Player Player { get; set; }

    [JsonPropertyName("friendly_fire")]
    public bool FriendlyFire { get; set; }

    [JsonPropertyName("flash_assist")]
    public bool FlashAssist { get; set; }

    public Assister(Player player, bool friendlyFire, bool flashAssist)
    {
        Player = player;
        FriendlyFire = friendlyFire;
        FlashAssist = flashAssist;
    }
}
