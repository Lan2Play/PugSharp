using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class PlayerBombEvent : PlayerTimedRoundEvent
{
    [JsonPropertyName("site_int")]
    public int Site { get; set; }

    protected PlayerBombEvent(string matchId, int mapNumber, int roundNumber, int roundTime, Player player, BombSite site, string eventName) : base(matchId, mapNumber, roundNumber, roundTime, player, eventName)
    {
        Site = (int)site;
    }
}