using SharpTournament.Match.Contract;

namespace SharpTournament.Match;

public class MatchTeam
{
    public MatchTeam(Team team)
    {
        Team = team;
    }

    public List<MatchPlayer> Players { get; } = new List<MatchPlayer>();
    public Team Team { get; }
}
