using SharpTournament.Match.Contract;

namespace SharpTournament.Match;

public class Vote
{
    public Vote(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public List<IPlayer> Votes { get; } = new List<IPlayer>();
}
