namespace PugSharp.Match.Contract
{
    public interface IPlayerStatistics
    {
        int Assists { get; set; }
        int BombDefuses { get; set; }
        int BombPlants { get; set; }
        bool Coaching { get; set; }
        int ContributionScore { get; set; }
        int Count1K { get; set; }
        int Count2K { get; set; }
        int Count3K { get; set; }
        int Count4K { get; set; }
        int Count5K { get; set; }
        int Damage { get; set; }
        int Deaths { get; set; }
        int EnemiesFlashed { get; set; }
        int FirstDeathCt { get; set; }
        int FirstDeathT { get; set; }
        int FirstKillCt { get; set; }
        int FirstKillT { get; set; }
        int FlashbangAssists { get; set; }
        int FriendliesFlashed { get; set; }
        int HeadshotKills { get; set; }
        int Kast { get; set; }
        int Kills { get; set; }
        int KnifeKills { get; set; }
        int Mvp { get; set; }
        string Name { get; set; }
        int RoundsPlayed { get; set; }
        int Suicides { get; set; }
        int TeamKills { get; set; }
        int TradeKill { get; set; }
        int UtilityDamage { get; set; }
        int V1 { get; set; }
        int V2 { get; set; }
        int V3 { get; set; }
        int V4 { get; set; }
        int V5 { get; set; }
    }
}