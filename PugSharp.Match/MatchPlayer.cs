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
            //Player.Clan = _IsReady ? "ready" : "not ready";

            var readyTag = _IsReady ? "[ready]" : "[not ready]";



            Player.PlayerName = readyTag + Player.PlayerName.Replace("[ready]", "", StringComparison.OrdinalIgnoreCase).Replace("[not ready]", "", StringComparison.OrdinalIgnoreCase);
        }
    }
}
