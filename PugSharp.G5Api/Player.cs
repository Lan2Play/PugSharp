using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public sealed class Player : PlayerBase
{
    [JsonPropertyName("side_int")]
    public int Side { get; set; }

    [JsonPropertyName("is_bot")]
    public bool IsBot { get; set; }

    [JsonPropertyName("user_id")]
    public int UserId { get; set; }

    public Player(string steamId, string name, int userId, Side side, bool isBot) : base(steamId, name)
    {
        UserId = userId;
        Side = (int)side;
        IsBot = isBot;
    }
}
