using PugSharp.Match.Contract;

namespace PugSharp
{
    internal class CurrentRoundState
    {
        public bool FirstDeathDone { get; set; }

        public bool FirstKillDone { get; set; }

        public bool TerroristsClutching { get; set; }

        public bool CounterTerroristsClutching { get; set; }

        private Dictionary<ulong, PlayerRoundStats> _PlayerStats = new Dictionary<ulong, PlayerRoundStats>();

        public IReadOnlyDictionary<ulong, PlayerRoundStats> PlayerStats => _PlayerStats;

        public void Reset()
        {
            FirstDeathDone = false;
            FirstKillDone = false;
            TerroristsClutching = false;
            CounterTerroristsClutching = false;
            _PlayerStats.Clear();
        }

        public PlayerRoundStats GetPlayerRoundStats(ulong steamId, string name)
        { 
            if(!_PlayerStats.ContainsKey(steamId))
            {
                _PlayerStats[steamId] = new PlayerRoundStats(name);
            }

            return _PlayerStats[steamId];
        }
    }
}
