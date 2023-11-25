using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api.Tests;


    internal class Register
    {
        [JsonPropertyName("steam_id")]
        public required string SteamId { get; init; }
    }
