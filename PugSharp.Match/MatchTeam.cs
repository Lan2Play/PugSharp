using PugSharp.Match.Contract;

namespace PugSharp.Match;

public class MatchTeam
{
    public MatchTeam(Team team, Config.Team teamConfig)
    {
        Team = team;
        TeamConfig = teamConfig;
    }

    public List<MatchPlayer> Players { get; } = new List<MatchPlayer>();

    public Team Team { get; set; }

    public Config.Team TeamConfig { get; }

    public bool IsPaused { get; internal set; }
}
