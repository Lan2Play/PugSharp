using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public class PlayerRoundEvent : RoundEvent
{

    [JsonPropertyName("player")]
    public Player Player { get; set; }

    protected PlayerRoundEvent(string matchId, int mapNumber, int roundNumber, Player player, string eventName) : base(matchId, mapNumber, roundNumber, eventName)
    {
        Player = player;
    }
}
