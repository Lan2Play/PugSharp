using PugSharp.Match.Contract;

namespace PugSharp.Match;

public class MatchInfo
{
    public MatchInfo(int numberOfMaps)
    {
        MatchMaps = Enumerable.Range(0, numberOfMaps).Select(n => new MatchMap(n)).ToList();
        CurrentMap = MatchMaps[0];
    }

    public MatchMap CurrentMap { get; set; }

    public IReadOnlyList<MatchMap> MatchMaps { get; }

}

public class MatchMap
{
    public MatchMap(int mapNumber)
    {
        MapNumber = mapNumber;
    }

    public int MapNumber { get; }

    public string MapName { get; set; }

    public MatchTeam? Winner { get; internal set; }

    public int Team1Points { get; set; }

    public int Team2Points { get; set; }
}
