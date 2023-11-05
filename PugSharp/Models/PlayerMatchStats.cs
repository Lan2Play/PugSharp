using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using PugSharp.Logging;
using PugSharp.Match.Contract;

namespace PugSharp.Models;

public class PlayerMatchStats : IPlayerMatchStats
{
    private static readonly ILogger<PlayerMatchStats> _Logger = LogManager.CreateLogger<PlayerMatchStats>();

    private readonly CSMatchStats_t _InternalMatchStats;

    private readonly IPlayer _Player;

    public PlayerMatchStats(CSMatchStats_t matchStats, IPlayer player)
    {
        _InternalMatchStats = matchStats;
        _Player = player;
    }

    public int Kills { get; set; }

    public int Assists { get; set; }

    public int Deaths { get; set; }

    public int Suicides { get; set; }

    public int FirstDeathCt { get; set; }

    public int FirstDeathT { get; set; }

    public int TeamKills { get; set; }

    public int FirstKillT { get; set; }

    public int FirstKillCt { get; set; }

    public int HeadshotKills { get; set; }

    public int KnifeKills { get; set; }

    public int FlashbangAssists { get; set; }

    public void ResetStats()
    {
        _Logger.LogInformation($"ResetStats for Player {_Player.PlayerName}. Kills: {Kills}; Assists: {Assists}; Deaths: {Deaths}");
        
        Kills = 0;
        Assists = 0;
        Deaths = 0;
        Suicides = 0;
        FirstDeathCt = 0;
        FirstDeathT = 0;
        TeamKills = 0;
        FirstKillT = 0;
        FirstKillCt = 0;
        HeadshotKills = 0;
        KnifeKills = 0;
        FlashbangAssists = 0;
        
        // TODO Alle Stats auf 0 setzen?
    }
}
