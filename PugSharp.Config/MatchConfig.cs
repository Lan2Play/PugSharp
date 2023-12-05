using System.Text.Json.Serialization;

namespace PugSharp.Config;

public class MatchConfig
{
    [JsonPropertyName("maplist")]
    public IList<string> Maplist { get; init; } = new List<string>();

    [JsonPropertyName("team1")]
    public required Team Team1 { get; init; }

    [JsonPropertyName("team2")]
    public required Team Team2 { get; init; }

    [JsonPropertyName("matchid")]
    public required string MatchId { get; init; }

    [JsonPropertyName("num_maps")]
    public int NumMaps { get; init; } = 1;

    [JsonPropertyName("players_per_team")]
    public int PlayersPerTeam { get; set; } = 5;

    [JsonPropertyName("min_players_to_ready")]
    public int MinPlayersToReady { get; set; } = 5;

    [JsonPropertyName("max_rounds")]
    public int MaxRounds { get; set; } = 24;

    [JsonPropertyName("max_overtime_rounds")]
    public int MaxOvertimeRounds { get; set; } = 6;

    [JsonPropertyName("vote_timeout")]
    public long VoteTimeout { get; init; } = 60000;

    [JsonPropertyName("eventula_apistats_url")]
    public string? EventulaApistatsUrl { get; init; }

    [JsonPropertyName("eventula_apistats_token")]
    public string? EventulaApistatsToken { get; set; }

    [JsonPropertyName("eventula_demo_upload_url")]
    public string? EventulaDemoUploadUrl { get; init; }

    [JsonPropertyName("allow_suicide")]
    public bool AllowSuicide { get; init; } = true;

    [JsonPropertyName("vote_map")]
    public string VoteMap { get; init; } = "de_dust2";

    [JsonPropertyName("team_mode")]
    public TeamMode TeamMode { get; set; }

    [JsonPropertyName("cvars")]
    public IDictionary<string, string> CVars { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    public string? G5ApiUrl => CVars.TryGetValue("get5_remote_log_url", out var value) ? value : null;

    public string? G5ApiHeader => CVars.TryGetValue("get5_remote_log_header_key", out var value) ? value : null;

    public string? G5ApiHeaderValue => CVars.TryGetValue("get5_remote_log_header_value", out var value) ? value : null;
}
