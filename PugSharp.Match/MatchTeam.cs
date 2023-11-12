using Microsoft.Extensions.Logging;
using PugSharp.Logging;
using PugSharp.Match.Contract;
using System.Text.Json.Serialization;

namespace PugSharp.Match;

public class MatchTeam
{
    private static readonly ILogger<MatchTeam> _Logger = LogManager.CreateLogger<MatchTeam>();

    public MatchTeam(Config.Team teamConfig)
    {
        TeamConfig = teamConfig;
    }

    [JsonIgnore]
    public List<MatchPlayer> Players { get; } = new List<MatchPlayer>();

    public Team StartingTeamSite { get; set; }

    public Team CurrentTeamSite { get; set; }

    public Config.Team TeamConfig { get; }

    [JsonIgnore]
    public bool IsPaused { get; internal set; }

    internal void ToggleTeamSite()
    {
        CurrentTeamSite = CurrentTeamSite == Team.Terrorist ? Team.CounterTerrorist : Team.Terrorist;
        _Logger.LogInformation("Toggle TeamSite for team {team} to {teamSite}", TeamConfig.Name, CurrentTeamSite);
    }
}
