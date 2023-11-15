namespace PugSharp.Api.Contract
{
    public interface IPlayerStatistics
    {
        int Assists { get;  }
        int BombDefuses { get; }
        int BombPlants { get; }
        bool Coaching { get; }
        int ContributionScore { get; }
        int Count1K { get; }
        int Count2K { get; }
        int Count3K { get; }
        int Count4K { get; }
        int Count5K { get; }
        int Damage { get; }
        int Deaths { get; }
        int EnemiesFlashed { get; }
        int FirstDeathCt { get; }
        int FirstDeathT { get; }
        int FirstKillCt { get; }
        int FirstKillT { get; }
        int FlashbangAssists { get; }
        int FriendliesFlashed { get; }
        int HeadshotKills { get; }
        int Kast { get; }
        int Kills { get; }
        int KnifeKills { get; }
        int Mvp { get; }
        string Name { get; }
        int RoundsPlayed { get; }
        int Suicides { get; }
        int TeamKills { get; }
        int TradeKill { get; }
        int UtilityDamage { get; }
        int V1 { get; }
        int V2 { get; }
        int V3 { get; }
        int V4 { get; }
        int V5 { get; }
    }
}