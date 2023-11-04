namespace PugSharp.Match.Contract;

public interface IRoundResults
{
    public ITeamRoundResults TRoundResult { get; }
    public ITeamRoundResults CTRoundResult { get; }
}
