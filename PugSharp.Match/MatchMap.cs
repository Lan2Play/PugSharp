using PugSharp.ApiStats;

namespace PugSharp.Match;

public class MatchMap
{
    public MatchMap(int mapNumber)
    {
        MapNumber = mapNumber;
    }

    public int MapNumber { get; }

    public string MapName { get; set; }

    public MatchTeam? Winner { get; set; }

    public int Team1Points { get; set; }

    public int Team2Points { get; set; }

    public Dictionary<ulong, PlayerMatchStatistics> PlayerMatchStatistics { get; init; } = new Dictionary<ulong, PlayerMatchStatistics>();
}