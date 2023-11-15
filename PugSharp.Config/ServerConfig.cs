using System.Text.Json.Serialization;

namespace PugSharp.Config;

public class ServerConfig
{
    [JsonPropertyName("admins")]
    public IDictionary<ulong, string> Admins { get; set; } = new Dictionary<ulong, string>();
}
