using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class MatchEvent : EventBase
{
    [JsonPropertyName("matchid")]
    public required string MatchId { get; init; }

    protected MatchEvent(string eventName) : base(eventName)
    {
    }
}
