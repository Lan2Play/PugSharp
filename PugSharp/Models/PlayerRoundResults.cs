using PugSharp.Match.Contract;

namespace PugSharp.Models
{
    internal sealed class PlayerRoundResults : IPlayerMatchStatistics
    {
        public bool Coaching { get; set; }

        public required string Name { get; init; }

        public int Kills { get; set; }

        public int Deaths { get; set; }

        public int Assists { get; set; }

        public int FlashbangAssists { get; set; }

        public int TeamKills { get; set; }

        public int Suicides { get; set; }

        public int Damage { get; set; }

        public int UtilityDamage { get; set; }

        public int EnemiesFlashed { get; set; }

        public int FriendliesFlashed { get; set; }

        public int KnifeKills { get; set; }

        public int HeadshotKills { get; set; }

        public int RoundsPlayed { get; set; }

        public int BombDefuses { get; set; }

        public int BombPlants { get; set; }

        public int Count1K { get; set; }

        public int Count2K { get; set; }

        public int Count3K { get; set; }

        public int Count4K { get; set; }

        public int Count5K { get; set; }

        public int V1 { get; set; }

        public int V2 { get; set; }

        public int V3 { get; set; }

        public int V4 { get; set; }

        public int V5 { get; set; }

        public int FirstKillT { get; set; }

        public int FirstKillCt { get; set; }

        public int FirstDeathT { get; set; }

        public int FirstDeathCt { get; set; }

        public int TradeKill { get; set; }

        public int Kast { get; set; }

        public int ContributionScore { get; set; }

        public int Mvp { get; set; }
    }
}
