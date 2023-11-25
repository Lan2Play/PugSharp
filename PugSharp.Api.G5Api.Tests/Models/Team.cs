using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api.Tests.Models;
internal class Team
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("flag")]
    public required string Flag { get; init; }

    [JsonPropertyName("logo_file")]
    public required string Logo { get; init; }

    [JsonPropertyName("tag")]
    public required string Tag { get; init; }

    [JsonPropertyName("public_team")]
    public required bool IsPublic { get; init; }

    [JsonPropertyName("auth_name")]
    public required Dictionary<string, PlayerAuth> Players { get; init; }
}
