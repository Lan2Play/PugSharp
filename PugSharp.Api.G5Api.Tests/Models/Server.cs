using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api.Tests.Models;


internal sealed class Server
{
    [JsonPropertyName("ip_string")]
    public required string IpString { get; init; }

    [JsonPropertyName("port")]
    public required int Port { get; init; }

    [JsonPropertyName("display_name")]
    public required string DisplayName { get; init; }

    [JsonPropertyName("rcon_password")]
    public required string RconPassword { get; init; }

    [JsonPropertyName("public_server")]
    public required bool PublicServer { get; init; }
}

