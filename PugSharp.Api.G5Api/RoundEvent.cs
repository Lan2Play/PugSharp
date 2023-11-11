using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class RoundEvent : MapEvent
{
    [JsonPropertyName("round_number")]
    public required int RoundNumber { get; init; }

    protected RoundEvent(string eventName) : base(eventName)
    {
    }
}
