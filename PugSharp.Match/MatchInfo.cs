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
    }

    [JsonIgnore]
    public MatchMap CurrentMap { get; set; }

    public IReadOnlyList<MatchMap> MatchMaps { get; init; }
    public string DemoFile { get; init; }
    public MatchConfig Config { get; }
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