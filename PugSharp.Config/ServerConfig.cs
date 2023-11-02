using System.Text.Json.Serialization;

namespace PugSharp.Config
{
    public class ServerConfig
    {
        [JsonPropertyName("admins")]
        public Dictionary<ulong, string> Admins { get; set; }
    }
}
