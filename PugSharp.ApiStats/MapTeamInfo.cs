

using PugSharp.Api.Contract;

namespace PugSharp.ApiStats;

public class MapTeamInfo : IMapTeamInfo
{
    public IReadOnlyDictionary<string, IPlayerStatistics> Players { get; set; } = new Dictionary<string, IPlayerStatistics>(StringComparer.OrdinalIgnoreCase);

    public int Score { get; set; }

    public int ScoreT { get; set; }

    public int ScoreCT { get; set; }

    public TeamSide StartingSide { get; set; }
}
