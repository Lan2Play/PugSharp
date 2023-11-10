using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class BombEvent : TimedRoundEvent
{
    [JsonPropertyName("site_int")]
    public int Site { get; set; }

    protected BombEvent(string matchId, int mapNumber, int roundNumber, int roundTime, BombSite site, string eventName) : base(matchId, mapNumber, roundNumber, roundTime, eventName)
    {
        Site = (int)site;
    }
}
