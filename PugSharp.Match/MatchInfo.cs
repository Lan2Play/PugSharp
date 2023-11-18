using PugSharp.Config;
using System.Text.Json.Serialization;

namespace PugSharp.Match;

public class MatchInfo
{
    public MatchInfo(MatchConfig config)
    {
        MatchMaps = Enumerable.Range(0, config.NumMaps).Select(n => new MatchMap(n)).ToList();
        CurrentMap = MatchMaps[0];
        Config = config;

        MatchTeam1 = new MatchTeam(Config.Team1) { CurrentTeamSite = Contract.Team.Terrorist };
        MatchTeam2 = new MatchTeam(Config.Team2) { CurrentTeamSite = Contract.Team.CounterTerrorist};
    }

    [JsonIgnore]
    public MatchMap CurrentMap { get; set; }

    public IReadOnlyList<MatchMap> MatchMaps { get; init; }
    public string? DemoFile { get; set; }
    public MatchConfig Config { get; }

    public MatchTeam MatchTeam1 { get; set; }

    public MatchTeam MatchTeam2 { get; set; }
}