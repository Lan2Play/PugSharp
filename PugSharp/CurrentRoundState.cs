namespace PugSharp;

internal sealed class CurrentRoundState
{
    private readonly Dictionary<ulong, PlayerRoundStats> _PlayerStats = new();

    public bool FirstDeathDone { get; set; }

    public bool FirstKillDone { get; set; }

    public bool TerroristsClutching { get; set; }

    public bool CounterTerroristsClutching { get; set; }


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
        if (!_PlayerStats.TryGetValue(steamId, out PlayerRoundStats? value))
        {
            value = new PlayerRoundStats(name);
            _PlayerStats[steamId] = value;
        }

        return value;
    }
}
