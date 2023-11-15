using PugSharp.Match.Contract;

namespace PugSharp.Models;

internal sealed class TeamRoundResults : ITeamRoundResults
{

    public int Score { get; set; }

    public int ScoreT { get; set; }

    public int ScoreCT { get; set; }
}
