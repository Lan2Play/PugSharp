using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api.Tests.Models;

internal sealed class MatchResponseMatch
{
    [JsonPropertyName("api_key")]
    public required string ApiKey { get; init; }
}