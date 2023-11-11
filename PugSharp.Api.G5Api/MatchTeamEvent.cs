using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class MatchTeamEvent : MatchEvent
{
    [JsonPropertyName("team_int")]
    public required int TeamNumber { get; init; }

    protected MatchTeamEvent(string eventName) : base(eventName)
    {
    }
}
