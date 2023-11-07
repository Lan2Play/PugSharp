
namespace PugSharp.ApiStats
{
    public class MapTeamInfo
    {
        public Dictionary<SteamId, PlayerStatistics> Players { get; set; }

        public int Score { get; set; }

        public int ScoreT { get; set; }

        public int ScoreCT { get; set; }

        public StartingSide StartingSide { get; set; }
    }
}
