using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class PlayerDeathEvent : PlayerWeaponEvent
{
    [JsonPropertyName("bomb")]
    public bool Bomb { get; set; }

    [JsonPropertyName("headshot")]
    public bool Headshot { get; set; }

    [JsonPropertyName("thru_smoke")]
    public bool ThruSmoke { get; set; }

    [JsonPropertyName("penetrated")]
    public int Penetrated { get; set; }

    [JsonPropertyName("attacker_blind")]
    public bool AttackerBlind { get; set; }

    [JsonPropertyName("no_scope")]
    public bool NoScope { get; set; }

    [JsonPropertyName("suicide")]
    public bool Suicide { get; set; }

    [JsonPropertyName("friendly_fire")]
    public bool FriendlyFire { get; set; }

    [JsonPropertyName("attacker")]
    public Player Attacker { get; set; }

    [JsonPropertyName("assist")]
    public Assister Assist { get; set; }

    public PlayerDeathEvent(string matchId, int mapNumber, int roundNumber, int roundTime, Player player, Weapon weapon,
        Player attacker, Assister assist, bool friendlyFire, bool suicide, bool noScope, bool attackerBlind, int penetrated, bool thruSmoke,
        bool headshot, bool bomb) : base(matchId, mapNumber, roundNumber, roundTime, player, weapon, "player_death")
    {
        Attacker = attacker;
        Assist = assist;
        FriendlyFire = friendlyFire;
        Suicide = suicide;
        NoScope = noScope;
        AttackerBlind = attackerBlind;
        Penetrated = penetrated;
        ThruSmoke = thruSmoke;
        Headshot = headshot;
        Bomb = bomb;
    }
}
