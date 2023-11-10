using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class PlayerTimedRoundEvent : TimedRoundEvent
{
    [JsonPropertyName("player")]
    public Player Player { get; set; }

    protected PlayerTimedRoundEvent(string matchId, int mapNumber, int roundNumber, int roundTime, Player player, string eventName) : base(matchId, mapNumber, roundNumber, roundTime, eventName)
    {
        Player = player;
    }
}
