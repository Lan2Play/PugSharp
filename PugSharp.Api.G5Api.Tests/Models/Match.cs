using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api.Tests.Models;

internal class Match
{
    [JsonPropertyName("server_id")]
    public required int ServerId { get; init; }

    [JsonPropertyName("team1_id")]
    public required int Team1Id { get; init; }

    [JsonPropertyName("team2_id")]
    public required int Team2Id { get; init; }

    [JsonPropertyName("max_maps")]
    public required int MaxMaps { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("skip_veto")]
    public required bool SkipVeto { get; init; }

    [JsonPropertyName("veto_mappool")]
    public required string VetoMappool { get; init; }

    [JsonPropertyName("ignore_server")]
    public required bool IgnoreServer { get; init; }
}
