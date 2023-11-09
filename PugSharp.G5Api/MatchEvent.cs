using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public class MatchEvent : EventBase
{
    [JsonPropertyName("matchid")]
    public string MatchId { get; set; }

    protected MatchEvent(string matchId, string eventName) : base(eventName)
    {
        MatchId = matchId;
    }
}
