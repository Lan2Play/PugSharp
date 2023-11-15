using PugSharp.Match.Contract;

namespace PugSharp.Match;

public class Vote
{
    public Vote(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public IList<IPlayer> Votes { get; } = new List<IPlayer>();
}
