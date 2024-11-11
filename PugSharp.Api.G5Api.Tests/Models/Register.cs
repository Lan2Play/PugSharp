using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api.Tests.Models;


internal sealed class Register
{
    [JsonPropertyName("steam_id")]
    public required string SteamId { get; init; }
}
