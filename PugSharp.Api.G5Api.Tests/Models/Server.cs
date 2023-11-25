using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api.Tests;


internal class Server
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

    /*
    [JsonPropertyName("flag")]
    public required string Flag { get; init; }

    [JsonPropertyName("gotv_port")]
    public required int GotvPort { get; init; }
    */
}

