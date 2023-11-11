using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class PlayerDisconnectedEvent : MatchEvent
{
    [JsonPropertyName("player")]
    public required Player Player { get; init; }

    public PlayerDisconnectedEvent() : base("player_disconnect")
    {
    }
}

