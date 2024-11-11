using System.Text.Json.Serialization;

using PugSharp.Match.Contract;

namespace PugSharp.Match;

public class MatchTeam
{
    public MatchTeam(Config.Team teamConfig)
    {
        TeamConfig = teamConfig;
    }

    [JsonIgnore]
    public IList<MatchPlayer> Players { get; } = new List<MatchPlayer>();

    public Team StartingTeamSide { get; set; }

    public Team CurrentTeamSide { get; set; }

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

    internal void ToggleTeamSide()
    {
        CurrentTeamSide = CurrentTeamSide == Team.Terrorist ? Team.CounterTerrorist : Team.Terrorist;
    }
}
