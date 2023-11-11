using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class BombEvent : TimedRoundEvent
{
    [JsonPropertyName("site_int")]
    public required int Site { get; init; }

    protected BombEvent(string eventName) : base(eventName)
    {
    }
}
