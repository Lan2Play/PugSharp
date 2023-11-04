namespace PugSharp.Match.Contract;

public interface ITeamRoundResults
{
    public IReadOnlyDictionary<ulong, IPlayerRoundResults> PlayerResults { get; }

    public int Score { get; }

    public int ScoreT { get; }

    public int ScoreCT { get; }
}
