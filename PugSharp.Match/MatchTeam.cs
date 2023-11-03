using PugSharp.Match.Contract;

namespace PugSharp.Match;

public class MatchTeam
{
    public MatchTeam(Config.Team teamConfig)
    {
        TeamConfig = teamConfig;
    }

    public List<MatchPlayer> Players { get; } = new List<MatchPlayer>();

    public Team StartTeam { get; set; }

    public Config.Team TeamConfig { get; }

    public bool IsPaused { get; internal set; }
}
