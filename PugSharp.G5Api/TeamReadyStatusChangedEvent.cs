using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public sealed class TeamReadyStatusChangedEvent : MatchTeamEvent
{
    [JsonPropertyName("ready")]
    public bool Ready { get; set; }

    [JsonPropertyName("game_state_int")]
    public int GameState { get; set; }

    public TeamReadyStatusChangedEvent(string matchId, int teamNumber, GameState gameState, bool ready) : base(matchId, teamNumber, "team_ready_status_changed")
    {
        Ready = ready;
        GameState = (int)gameState;
    }
}
