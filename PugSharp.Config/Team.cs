using System.Text.Json.Serialization;

namespace PugSharp.Config;

public class Team
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("tag")]
    public string Tag { get; init; } = string.Empty;

    [JsonPropertyName("flag")]
    public string Flag { get; init; } = string.Empty;

    [JsonPropertyName("players")]
    public IDictionary<ulong, string> Players { get; init; } = new Dictionary<ulong, string>();
}
