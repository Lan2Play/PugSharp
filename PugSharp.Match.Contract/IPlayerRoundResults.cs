namespace PugSharp.Match.Contract;

public interface IPlayerRoundResults
{
    int Assists { get; }
    bool BombDefused { get; }
    bool BombPlanted { get; }
    bool Clutched { get; }
    int ClutchKills { get; }
    int Damage { get; }
    bool Dead { get; }
    int EnemiesFlashed { get; }
    bool FirstDeathCt { get; }
    bool FirstDeathT { get; }
    bool FirstKillCt { get; }
    bool FirstKillT { get; }
    int FlashbangAssists { get; }
    int FriendliesFlashed { get; }
    int HeadshotKills { get; }
    int Kills { get; }
    int KnifeKills { get; }
    bool Mvp { get; }
    bool Suicide { get; }
    int TeamKills { get; }
    int UtilityDamage { get; }
    int TradeKills { get; }

    int ContributionScore { get; }

    string Name { get; }
}