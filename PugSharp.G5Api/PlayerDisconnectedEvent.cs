using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public sealed class PlayerDisconnectedEvent : MatchEvent
{
    [JsonPropertyName("player")]
    public Player Player { get; set; }

    public PlayerDisconnectedEvent(string matchId, Player player) : base(matchId, "player_disconnect")
    {
        Player = player;
    }
}

