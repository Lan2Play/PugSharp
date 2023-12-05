using PugSharp.Match.Contract;

namespace PugSharp.Match;

public class MatchPlayer
{
    private bool _IsReady;

    public MatchPlayer(IPlayer player)
    {
        Player = player;
    }

    public IPlayer Player { get; }

    public bool IsReady
    {
        get => _IsReady; set
        {
            _IsReady = value;
        }
    }
}
