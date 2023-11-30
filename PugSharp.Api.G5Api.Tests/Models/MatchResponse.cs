using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api.Tests.Models;
internal sealed class MatchResponse
{
    [JsonPropertyName("match")]
    public required MatchResponseMatch Match { get; init; }
}
