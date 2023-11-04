using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using PugSharp.Logging;
using PugSharp.Match.Contract;

namespace PugSharp.Models;

public class PlayerMatchStats : IPlayerMatchStats
{
    private static readonly ILogger<PlayerMatchStats> _Logger = LogManager.CreateLogger<PlayerMatchStats>();

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
        set
        {
            if (_MatchStats.Kills != value)
            {
                _MatchStats.Kills = value;
            }
        }
    }

    public int Assists
    {
        get => _MatchStats.Assists;
        set
        {
            if (_MatchStats.Assists != value)
            {
                _MatchStats.Assists = value;
            }
        }
    }

    public int Deaths
    {
        get => _MatchStats.Deaths;
        set
        {
            if (_MatchStats.Deaths != value)
            {
                _MatchStats.Deaths = value;
            }
        }
    }

    public void ResetStats()
    {
        _Logger.LogInformation($"ResetStats for Player {_Player.PlayerName}. Kills: {Kills}; Assists: {Assists}; Deaths: {Deaths}");
        Kills = 0;
        Assists = 0;
        Deaths = 0;

        // TODO Alle Stats auf 0 setzen?
    }
}
