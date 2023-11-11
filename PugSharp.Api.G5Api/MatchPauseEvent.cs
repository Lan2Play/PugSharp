using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class MatchPauseEvent : MapTeamEvent
{
    [JsonPropertyName("pause_type_int")]
    public required int PauseType { get; init; }

    protected MatchPauseEvent(string eventName) : base(eventName)
    {
    }
}
