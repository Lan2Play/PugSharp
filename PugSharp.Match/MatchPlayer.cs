using PugSharp.Match.Contract;

namespace PugSharp.Match;

public class MatchPlayer
{
    public MatchPlayer(IPlayer player)
    {
        Player = player;
    }

    public IPlayer Player { get; }

    public bool IsReady { get; set; }
}
