using System.Text.Json.Serialization;

namespace PugSharp.Config;

public class ServerConfig
{
    [JsonPropertyName("locale")]
    public string Locale { get; init; } = "en";

    [JsonPropertyName("allow_players_without_match")]
    public bool AllowPlayersWithoutMatch { get; init; } = true;

    [JsonPropertyName("autoload_config")]
    public string AutoloadConfig { get; init; } = string.Empty;

    [JsonPropertyName("autoload_config_auth_token")]
    public string AutoloadConfigAuthToken { get; init; } = string.Empty;
}
