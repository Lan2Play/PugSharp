using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class BombDefusedEvent : PlayerBombEvent
{
    [JsonPropertyName("bomb_time_remaining")]
    public int TimeRemaining { get; set; }

    public BombDefusedEvent(string matchId, int mapNumber, int roundNumber, int roundTime, Player player, BombSite site, int timeRemaining) : base(matchId, mapNumber, roundNumber, roundTime, player, site, "bomb_defused")
    {
        TimeRemaining = timeRemaining;
    }
}
