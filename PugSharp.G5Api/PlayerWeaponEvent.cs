using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public class PlayerWeaponEvent : PlayerTimedRoundEvent
{
    [JsonPropertyName("weapon")]
    public Weapon Weapon { get; set; }
    
    protected PlayerWeaponEvent(string matchId, int mapNumber, int roundNumber, int roundTime, Player player, Weapon weapon, string eventName) : base(matchId, mapNumber, roundNumber, roundTime, player, eventName)
    {
        Weapon = weapon;
    }
}
