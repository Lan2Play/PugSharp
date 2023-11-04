using PugSharp.Match.Contract;

namespace PugSharp.Models
{
    internal class RoundResult : IRoundResults
    {
        public ITeamRoundResults TRoundResult { get; set; }

        public ITeamRoundResults CTRoundResult { get; set; }
    }
}
