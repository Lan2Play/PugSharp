using PugSharp.Match.Contract;

namespace PugSharp.Match;

public class MatchTeam
{
    public MatchTeam(Team team)
    {
        Team = team;
    }

    public List<MatchPlayer> Players { get; } = new List<MatchPlayer>();

    public Team Team { get; }

    public bool IsPaused { get; internal set; }
}
