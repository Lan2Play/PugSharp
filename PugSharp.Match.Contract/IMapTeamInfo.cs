
namespace PugSharp.Match.Contract
{
    public interface IMapTeamInfo
    {
        IReadOnlyDictionary<string, IPlayerStatistics> Players { get; set; }

        int Score { get; set; }
        int ScoreCT { get; set; }
        int ScoreT { get; set; }
        StartingSide StartingSide { get; set; }
    }
}