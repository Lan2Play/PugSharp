using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class PlayerSayEvent : PlayerTimedRoundEvent
{
    [JsonPropertyName("command")]
    public required string Command { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }

    public PlayerSayEvent() : base( "player_say")
    {
    }
}
