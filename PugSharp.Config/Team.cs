using System.Text.Json.Serialization;

namespace PugSharp.Config;

public class Team
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Ulid.NewUlid().ToString();

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("tag")]
    public string Tag { get; init; } = string.Empty;

    [JsonPropertyName("flag")]
    public string Flag { get; init; } = "DE";

    [JsonPropertyName("players")]
    public IDictionary<ulong, string> Players { get; init; } = new Dictionary<ulong, string>();
}
