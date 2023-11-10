

using PugSharp.Match.Contract;

namespace PugSharp.ApiStats
{
    public class MapTeamInfo : IMapTeamInfo
    {
        public IReadOnlyDictionary<string, IPlayerStatistics> Players { get; set; }

        public int Score { get; set; }

        public int ScoreT { get; set; }

        public int ScoreCT { get; set; }

        public StartingSide StartingSide { get; set; }
    }
}
