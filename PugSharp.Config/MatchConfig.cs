using System.Text.Json.Serialization;

namespace PugSharp.Config
{
    public class MatchConfig
    {
        [JsonPropertyName("maplist")]
        public string[] Maplist { get; set; }

        [JsonPropertyName("team1")]
        public Team Team1 { get; set; }

        [JsonPropertyName("team2")]
        public Team Team2 { get; set; }

        [JsonPropertyName("matchid")]
        public string MatchId { get; set; }

        [JsonPropertyName("num_maps")]
        public int NumMaps { get; set; } = 1;

        [JsonPropertyName("players_per_team")]
        public int PlayersPerTeam { get; set; } = 5;

        [JsonPropertyName("min_players_to_ready")]
        public int MinPlayersToReady { get; set; } = 5;

        [JsonPropertyName("max_rounds")]
        public int MaxRounds { get; set; } = 24;

        [JsonPropertyName("max_overtime_rounds")]
        public int MaxOvertimeRounds { get; set; } = 6;

        [JsonPropertyName("vote_timeout")]
        public long VoteTimeout { get; set; } = 60000;

        [JsonPropertyName("eventula_apistats_url")]
        public string? EventulaApistatsUrl { get; set; }

        [JsonPropertyName("eventula_apistats_token")]
        public string? EventulaApistatsToken { get; set; }

        [JsonPropertyName("eventula_demo_upload_url")]
        public string? EventulaDemoUploadUrl { get; set; }

        [JsonPropertyName("g5_api_url")]
        public string? G5ApiUrl { get; set; }

        [JsonPropertyName("g5_api_header")]
        public string? G5ApiHeader { get; set; }

        [JsonPropertyName("g5_api_headervalue")]
        public string? G5ApiHeaderValue { get; set; }

    }
}
