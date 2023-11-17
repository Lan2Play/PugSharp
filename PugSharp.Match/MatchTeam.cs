using PugSharp.Match.Contract;
using System.Text.Json.Serialization;

namespace PugSharp.Match;

public class MatchTeam
{
    public MatchTeam(Config.Team teamConfig)
    {
        TeamConfig = teamConfig;
    }

    [JsonIgnore]
    public IList<MatchPlayer> Players { get; } = new List<MatchPlayer>();

    public Team StartingTeamSite { get; set; }

    public Team CurrentTeamSite { get; set; }

    public Config.Team TeamConfig { get; }

    [JsonIgnore]
    public bool IsPaused { get; internal set; }


    internal void PrintToChat(string message)
    {
        foreach (var player in Players)
        {
            player.Player.PrintToChat(message);
        }
    }

    internal void ToggleTeamSite()
    {
        CurrentTeamSite = CurrentTeamSite == Team.Terrorist ? Team.CounterTerrorist : Team.Terrorist;
    }
}
