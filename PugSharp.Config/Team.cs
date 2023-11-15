using System.Text.Json.Serialization;

namespace PugSharp.Config
{
    public class Team
    {
        [JsonPropertyName("name")]
        public required string Name { get; init; }

        [JsonPropertyName("tag")]
        public string Tag { get; init; } = string.Empty;

        [JsonPropertyName("flag")]
        public string Flag { get; init; } = string.Empty;

        [JsonPropertyName("players")]
        public required Dictionary<ulong, string> Players { get; init; }
    }
}
