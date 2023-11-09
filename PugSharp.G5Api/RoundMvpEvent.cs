using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public sealed class RoundMvpEvent : PlayerRoundEvent
{
    [JsonPropertyName("reason")]
    public int Reason { get; set; }

    public RoundMvpEvent(string matchId, int mapNumber, int roundNumber, Player player, int reason) : base(matchId, mapNumber, roundNumber, player, "round_mvp")
    {
        Reason = reason;
    }
}
