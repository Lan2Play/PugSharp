using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public sealed class GameStateChangedEvent : EventBase
{
    [JsonPropertyName("new_state_int")]
    public int NewState { get; set; }

    [JsonPropertyName("old_state_int")]
    public int OldState { get; set; }

    public GameStateChangedEvent(GameState newState, GameState oldState) : base("game_state_changed")
    {
        NewState = (int)newState;
        OldState = (int)oldState;
    }
}
