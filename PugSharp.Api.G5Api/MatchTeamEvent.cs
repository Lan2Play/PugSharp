using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class MatchTeamEvent : MatchEvent
{
    /// <summary>
    /// Possible Values
    /// - team1
    /// - team2
    /// </summary>
    [JsonPropertyName("team")]
    public required string Team { get; init; }

    protected MatchTeamEvent(string eventName) : base(eventName)
    {
    }
}
