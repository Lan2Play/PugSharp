using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class PlayerBombEvent : PlayerTimedRoundEvent
{
    [JsonPropertyName("site_int")]
    public required int Site { get; init; }

    protected PlayerBombEvent(string eventName) : base(eventName)
    {
    }
}