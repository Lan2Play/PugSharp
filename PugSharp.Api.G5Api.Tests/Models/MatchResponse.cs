using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api.Tests.Models;
internal class MatchResponse
{
    [JsonPropertyName("match")]
    public required MatchResponseMatch Match { get; init; }


}

internal class MatchResponseMatch
{
    [JsonPropertyName("api_key")]
    public required string ApiKey { get; init; }
}