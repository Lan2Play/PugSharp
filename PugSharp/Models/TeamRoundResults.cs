using PugSharp.Match.Contract;

namespace PugSharp.Models
{
    internal class TeamRoundResults : ITeamRoundResults
    {
        public IReadOnlyDictionary<ulong, IPlayerRoundResults> PlayerResults { get; set; }

        public int Score { get; set; }

        public int ScoreT { get; set; }

        public int ScoreCT { get; set; }
    }
}
