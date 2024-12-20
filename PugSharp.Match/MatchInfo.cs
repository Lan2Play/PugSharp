﻿using System.Text.Json.Serialization;

using PugSharp.Config;

namespace PugSharp.Match;

public class MatchInfo
{
    public MatchInfo(MatchConfig config)
    {
        Config = config;
        MatchMaps = Enumerable.Range(0, Config.NumMaps).Select(n => new MatchMap(n)).ToList();
        CurrentMap = MatchMaps[0];

        MatchTeam1 = new MatchTeam(Config.Team1) { CurrentTeamSide = Contract.Team.Terrorist };
        MatchTeam2 = new MatchTeam(Config.Team2) { CurrentTeamSide = Contract.Team.CounterTerrorist };
        RandomPlayersAllowed = Config.Team1.Players.Count == 0 && Config.Team2.Players.Count == 0;
    }

    public bool RandomPlayersAllowed { get; }

    [JsonIgnore]
    public MatchMap CurrentMap { get; set; }

    public IReadOnlyList<MatchMap> MatchMaps { get; init; }
    public string? DemoFile { get; set; }
    public MatchConfig Config { get; }

    public MatchTeam MatchTeam1 { get; set; }

    public MatchTeam MatchTeam2 { get; set; }
}