using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class PlayerConnectedEvent : MatchEvent
{

    [JsonPropertyName("ip_address")]
    public required string IpAddress { get; init; }

    [JsonPropertyName("player")]
    public required Player Player { get; init; }

    public PlayerConnectedEvent() : base("player_connect")
    {
    }
}
