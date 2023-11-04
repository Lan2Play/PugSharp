namespace PugSharp.Match.Contract;

public interface IPlayerRoundResults
{
    public bool Coaching { get; }

    public string Name { get; }

    public int Kills { get; }

    public int Deaths { get; }

    public int Assists { get; }

    public int FlashbangAssists { get; }

    public int TeamKills { get; }

    public int Suicides { get; }

    public int Damage { get; }

    public int UtilityDamage { get; }

    public int EnemiesFlashed { get; }

    public int FriendliesFlashed { get; }

    public int KnifeKills { get; }

    public int HeadshotKills { get; }

    public int RoundsPlayed { get; }

    public int BombDefuses { get; }

    public int BombPlants { get; }

    public int Count1K { get; }

    public int Count2K { get; }

    public int Count3K { get; }

    public int Count4K { get; }

    public int Count5K { get; }

    public int V1 { get; }

    public int V2 { get; }

    public int V3 { get; }

    public int V4 { get; }

    public int V5 { get; }

    public int FirstKillT { get; }

    public int FirstKillCt { get; }

    public int FirstDeathT { get; }

    public int FirstDeathCt { get; }

    public int TradeKill { get; }

    public int Kast { get; }

    public int ContributionScore { get; }

    public int Mvp { get; }
}