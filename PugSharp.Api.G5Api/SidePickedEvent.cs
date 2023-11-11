using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class SidePickedEvent : MapSelectionEvent
{
    [JsonPropertyName("map_number")]
    public required int MapNumber { get; init; }

    [JsonPropertyName("side_int")]
    public required int Side { get; init; }

    public SidePickedEvent() : base("side_picked")
    {
    }
}
