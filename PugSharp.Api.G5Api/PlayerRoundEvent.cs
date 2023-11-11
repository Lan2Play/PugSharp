using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class PlayerRoundEvent : RoundEvent
{

    [JsonPropertyName("player")]
    public required Player Player { get; init; }

    protected PlayerRoundEvent(string eventName) : base(eventName)
    {
    }
}
