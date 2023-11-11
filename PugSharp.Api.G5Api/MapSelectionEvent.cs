using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class MapSelectionEvent : MatchTeamEvent
{
    [JsonPropertyName("map_name")]
    public required string MapName { get; init; }

    protected MapSelectionEvent(string eventName) : base(eventName)
    {
    }
}
