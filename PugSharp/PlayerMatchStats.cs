using CounterStrikeSharp.API.Core;
using PugSharp.Match.Contract;

namespace PugSharp;

public class PlayerMatchStats : IPlayerMatchStats
{
    private readonly CSMatchStats_t _MatchStats;
    private readonly IPlayer _Player;

    public PlayerMatchStats(CSMatchStats_t matchStats, IPlayer player)
    {
        _MatchStats = matchStats;
        _Player = player;
    }

    public int Kills
    {
        get => _MatchStats.Kills;
        set => _MatchStats.Kills = value;
    }

    public int Assists
    {
        get => _MatchStats.Assists;
        set => _MatchStats.Assists = value;
    }
    public int Deaths
    {
        get => _MatchStats.Deaths;
        set => _MatchStats.Deaths = value;
    }


    public void ResetStats()
    {
        Console.WriteLine($"ResetStats for Player {_Player.PlayerName}. Kills: {Kills}; Assists: {Assists}; Deaths: {Deaths}");
        Kills = 0;
        Assists = 0;
        Deaths = 0;

        // TODO Alle Stats auf 0 setzen?
    }
}
