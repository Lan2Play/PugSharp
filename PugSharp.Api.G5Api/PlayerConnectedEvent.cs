using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public sealed class PlayerConnectedEvent : MatchEvent
{

    [JsonPropertyName("ip_address")]
    public string IpAddress { get; set; }

    [JsonPropertyName("player")]
    public Player Player { get; set; }

    public PlayerConnectedEvent(string matchId, Player player, string ipAddress) : base(matchId, "player_connect")
    {
        Player = player;
        IpAddress = ipAddress;
    }
}
