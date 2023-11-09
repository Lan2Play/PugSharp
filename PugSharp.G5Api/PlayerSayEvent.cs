using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public sealed class PlayerSayEvent : PlayerTimedRoundEvent
{
    [JsonPropertyName("command")]
    public string Command { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    public PlayerSayEvent(string matchId, int mapNumber, int roundNumber, int roundTime, Player player, string message, string command) : base(matchId, mapNumber, roundNumber, roundTime, player, "player_say")
    {
        Command = command;
        Message = message;
    }
}
