using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api.Tests.Models;

public class PlayerAuth
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("captain")]
    public required bool IsCaptain { get; init; }

    [JsonPropertyName("coach")]
    public required bool IsCoach { get; init; }
}