namespace PugSharp.Match.Contract;

public interface IRoundResults
{
    public ITeamRoundResults TRoundResult { get; }
    public ITeamRoundResults CTRoundResult { get; }

    public IReadOnlyDictionary<ulong, IPlayerRoundResults> PlayerResults { get; }
}
