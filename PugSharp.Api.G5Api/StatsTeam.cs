using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace PugSharp.Api.G5Api;

public class StatsTeam : TeamWrapper
{
    [JsonPropertyName("series_score")]
    public int SeriesScore { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }

    public int ScoreCt { get; }

    [JsonPropertyName("score_ct")]
    public int ScoreCT { get; set; }

    [JsonPropertyName("score_t")]
    public int ScoreT { get; set; }

    [JsonPropertyName("players")]
    public IEnumerable<StatsPlayer> Players { get; set; } = new List<StatsPlayer>();

    public StatsTeam(string id, string name, int seriesScore, int score, int scoreCt, int scoreT, IEnumerable<StatsPlayer> players) : base(id, name)
    {
        SeriesScore = seriesScore;
        Score = score;
        ScoreCt = scoreCt;
        ScoreT = scoreT;
        Players = players;
    }
}
