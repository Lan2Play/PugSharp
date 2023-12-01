using System.Text.Json.Serialization;

namespace PugSharp.Config;

public class ServerConfig
{
    [JsonPropertyName("locale")]
    public string Locale { get; init; } = "en";

    [JsonPropertyName("allow_players_without_match")]
    public bool AllowPlayersWithoutMatch { get; init; } = true;
}
