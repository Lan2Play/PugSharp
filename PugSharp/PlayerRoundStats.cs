using PugSharp.Match.Contract;

namespace PugSharp
{
    internal class PlayerRoundStats : IPlayerRoundResults
    {
        public int Kills { get; set; }

        public bool Clutched { get; set; }

        public int ClutchKills { get; set; }

        public bool Dead { get; set; }

        public int Assists { get; set; }

        public int FlashbangAssists { get; set; }

        public int TeamKills { get; set; }

        public bool Suicide { get; set; }

        public int Damage { get; set; }

        public int UtilityDamage { get; set; }

        public int EnemiesFlashed { get; set; }

        public int FriendliesFlashed { get; set; }

        public int KnifeKills { get; set; }

        public int HeadshotKills { get; set; }

        public bool BombDefused { get; set; }

        public bool BombPlanted { get; set; }

        public bool FirstKillT { get; set; }

        public bool FirstKillCt { get; set; }

        public bool FirstDeathT { get; set; }

        public bool FirstDeathCt { get; set; }

        public bool Mvp { get; set; }

        public int TradeKills { get; set; }

        public string Name { get;  }

        public PlayerRoundStats(string name)
        {
            Name = name;
        }
    }

}
