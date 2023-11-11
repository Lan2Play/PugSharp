using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class PlayerTimedRoundEvent : TimedRoundEvent
{
    [JsonPropertyName("player")]
    public required Player Player { get; init; }

    protected PlayerTimedRoundEvent(string eventName) : base(eventName)
    {
    }
}
