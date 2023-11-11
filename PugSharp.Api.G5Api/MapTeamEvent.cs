using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class MapTeamEvent : MapEvent
{
    [JsonPropertyName("team_int")]
    public required int TeamNumber { get; init; }

    protected MapTeamEvent(string eventName) : base(eventName)
    {
    }
}

