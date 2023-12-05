using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class StatsPlayer
{
    [JsonPropertyName("steamid")]
    public required string SteamId { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("stats")]
    public required PlayerStats Stats { get; init; }
}