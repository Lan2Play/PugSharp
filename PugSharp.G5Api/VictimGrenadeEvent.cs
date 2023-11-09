using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public class VictimGrenadeEvent : PlayerWeaponEvent
{
    [JsonPropertyName("victims")]
    public IEnumerable<GrenadeVictim> Victims { get; set; } = new List<GrenadeVictim>();

    protected VictimGrenadeEvent(string matchId, int mapNumber, int roundNumber, int roundTime, Player player, Weapon weapon, IEnumerable<GrenadeVictim> victims, string eventName) : base(matchId, mapNumber, roundNumber, roundTime, player, weapon, eventName)
    {
        Victims = victims;
    }
}
