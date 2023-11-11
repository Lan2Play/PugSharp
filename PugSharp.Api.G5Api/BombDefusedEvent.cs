using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class BombDefusedEvent : PlayerBombEvent
{
    [JsonPropertyName("bomb_time_remaining")]
    public required int TimeRemaining { get; init; }

    public BombDefusedEvent() : base( "bomb_defused")
    {
    }
}
