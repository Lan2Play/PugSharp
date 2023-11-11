using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class MapEvent : MatchEvent
{
    [JsonPropertyName("map_number")]
    public required int MapNumber { get; init; }

    protected MapEvent(string eventName) : base(eventName)
    {
    }
}
