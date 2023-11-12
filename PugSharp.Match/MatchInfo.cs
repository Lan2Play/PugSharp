using PugSharp.ApiStats;
using PugSharp.Config;
using System.Text.Json.Serialization;

namespace PugSharp.Match;

public class MatchInfo
{
    public MatchInfo(Config.MatchConfig config)
    {
        MatchMaps = Enumerable.Range(0, config.NumMaps).Select(n => new MatchMap(n)).ToList();
        CurrentMap = MatchMaps[0];
        Config = config;

        MatchTeam1 = new MatchTeam(Config.Team1);
        MatchTeam2 = new MatchTeam(Config.Team2);
    }

    [JsonIgnore]
    public MatchMap CurrentMap { get; set; }

    public IReadOnlyList<MatchMap> MatchMaps { get; init; }
    public string DemoFile { get; set; }
    public MatchConfig Config { get; }

    public MatchTeam MatchTeam1 { get; set; }

    public MatchTeam MatchTeam2 { get; set; }
}

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