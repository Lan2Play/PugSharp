
namespace PugSharp.Api.Contract
{
    public interface IMapTeamInfo
    {
        IReadOnlyDictionary<string, IPlayerStatistics> Players { get; }

        int Score { get; }
        int ScoreCT { get; }
        int ScoreT { get; }
        StartingSide StartingSide { get; }
    }
}