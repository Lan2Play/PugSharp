namespace PugSharp.Match.Contract;

public interface IRoundResults
{
    Team RoundWinner { get; }

    ITeamRoundResults TRoundResult { get; }

    ITeamRoundResults CTRoundResult { get; }

    IReadOnlyDictionary<ulong, IPlayerRoundResults> PlayerResults { get; }
    int Reason { get; }
}
