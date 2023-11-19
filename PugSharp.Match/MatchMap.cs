namespace PugSharp.Match;

public class MatchMap
{
    public MatchMap(int mapNumber)
    {
        MapNumber = mapNumber;
    }

    public int MapNumber { get; }

    public string MapName { get; set; } = string.Empty;

    public MatchTeam? Winner { get; set; }

    public int Team1Points { get; set; }

    public int Team2Points { get; set; }

    public IDictionary<ulong, PlayerMatchStatistics> PlayerMatchStatistics { get; init; } = new Dictionary<ulong, PlayerMatchStatistics>();
}