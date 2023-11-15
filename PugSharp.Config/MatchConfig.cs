using System.Text.Json.Serialization;

namespace PugSharp.Config
{
    public class MatchConfig
    {
        [JsonPropertyName("maplist")]
        public required string[] Maplist { get; init; }

        [JsonPropertyName("team1")]
        public required Team Team1 { get; init; }

        [JsonPropertyName("team2")]
        public required Team Team2 { get; init; }

        [JsonPropertyName("matchid")]
        public required string MatchId { get; init; }

        [JsonPropertyName("num_maps")]
        public int NumMaps { get; init; } = 1;

        [JsonPropertyName("players_per_team")]
        public int PlayersPerTeam { get; init; } = 5;

        [JsonPropertyName("min_players_to_ready")]
        public int MinPlayersToReady { get; init; } = 5;

        [JsonPropertyName("max_rounds")]
        public int MaxRounds { get; init; } = 24;

        [JsonPropertyName("max_overtime_rounds")]
        public int MaxOvertimeRounds { get; init; } = 6;

        [JsonPropertyName("vote_timeout")]
        public long VoteTimeout { get; init; } = 60000;

        [JsonPropertyName("eventula_apistats_url")]
        public string? EventulaApistatsUrl { get; init; }

        [JsonPropertyName("eventula_apistats_token")]
        public string? EventulaApistatsToken { get; set; }

        [JsonPropertyName("eventula_demo_upload_url")]
        public string? EventulaDemoUploadUrl { get; init; }

        [JsonPropertyName("g5_api_url")]
        public string? G5ApiUrl { get; init; }

        [JsonPropertyName("g5_api_header")]
        public string? G5ApiHeader { get; init; }

        [JsonPropertyName("g5_api_headervalue")]
        public string? G5ApiHeaderValue { get; init; }

        [JsonPropertyName("allow_suicide")]
        public bool AllowSuicide { get; init; } = true;

        [JsonPropertyName("vote_map")]
        public string VoteMap { get; init; } = "de_dust2";

        [JsonPropertyName("server_locale")]
        public string ServerLocale { get; init; } = "en";
    }
}
