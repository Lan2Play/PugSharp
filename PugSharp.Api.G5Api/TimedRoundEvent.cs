using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class TimedRoundEvent : RoundEvent
{
    [JsonPropertyName("round_time")]
    public required int RoundTime { get; init; }

    protected TimedRoundEvent(string eventName) : base(eventName)
    {
    }
}
