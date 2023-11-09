using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public class RoundEvent : MapEvent
{
    [JsonPropertyName("round_number")]
    public int RoundNumber { get; set; }


    protected RoundEvent(string matchId, int mapNumber, int roundNumber, string eventName) : base(matchId, mapNumber, eventName)
    {
        RoundNumber = roundNumber;
    }
}
