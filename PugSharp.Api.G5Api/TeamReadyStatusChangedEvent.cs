using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class TeamReadyStatusChangedEvent : MatchTeamEvent
{
    [JsonPropertyName("ready")]
    public required bool Ready { get; init; }

    [JsonPropertyName("game_state_int")]
    public required int GameState { get; init; }

    public TeamReadyStatusChangedEvent() : base("team_ready_status_changed")
    {
    }
}
