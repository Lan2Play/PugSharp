using System.Text.Json.Serialization;

namespace PugSharp.Config;

public class ServerConfig
{
    [JsonPropertyName("locale")]
    public string Locale { get; init; } = "en";
}
