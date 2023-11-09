using System.Text.Json.Serialization;

namespace PugSharp.G5Api;

public sealed class SeriesResultEvent : MatchEvent
{
    [JsonPropertyName("time_until_restore")]
    public int TimeUntilRestore { get; set; }

    [JsonPropertyName("winner")]
    public Winner Winner { get; set; }

    [JsonPropertyName("team1_series_score")]
    public int Team1SeriesScore { get; set; }

    [JsonPropertyName("team2_series_score")]
    public int Team2SeriesScore { get; set; }

    public SeriesResultEvent(string matchId, Winner winner, int team1SeriesScore, int team2SeriesScore, int timeUntilRestore) : base(matchId, "series_end")
    {
        TimeUntilRestore = timeUntilRestore;
        Winner = winner;
        Team1SeriesScore = team1SeriesScore;
        Team2SeriesScore = team2SeriesScore;
    }
}
