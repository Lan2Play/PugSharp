using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class TimedRoundEvent : RoundEvent
{
    [JsonPropertyName("round_time")]
    public int RoundTime { get; set; }

    protected TimedRoundEvent(string matchId, int mapNumber, int roundNumber, int roundTime, string eventName) : base(matchId, mapNumber, roundNumber, eventName)
    {
        RoundTime = roundTime;
    }
}
