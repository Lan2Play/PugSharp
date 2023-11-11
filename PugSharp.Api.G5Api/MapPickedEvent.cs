using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class MapPickedEvent : MapSelectionEvent
{
    [JsonPropertyName("map_number")]
    public required int MapNumber { get; init; }

    public MapPickedEvent() : base("map_picked")
    {
    }
}
