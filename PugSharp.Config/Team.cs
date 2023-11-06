using System.Text.Json.Serialization;

namespace PugSharp.Config
{
    public class Team
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; } = string.Empty;

        [JsonPropertyName("flag")]
        public string Flag { get; set; } = string.Empty;

        [JsonPropertyName("players")]
        public Dictionary<ulong, string> Players { get; set; }
    }
}
