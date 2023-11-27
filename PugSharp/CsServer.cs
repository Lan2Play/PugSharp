using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;

using Microsoft.Extensions.Logging;

using PugSharp.Match.Contract;
using PugSharp.Models;
using PugSharp.Server.Contract;

namespace PugSharp;

public class CsServer : ICsServer
{
    private readonly ILogger<CsServer> _Logger;

    public CsServer(ILogger<CsServer> logger)
    {
        _Logger = logger;
    }

    public string GameDirectory => CounterStrikeSharp.API.Server.GameDirectory;

    public void ExecuteCommand(string v)
    {
        CounterStrikeSharp.API.Server.ExecuteCommand(v);
    }

    public bool IsMapValid(string selectedMap)
    {
        return CounterStrikeSharp.API.Server.IsMapValid(selectedMap);
    }

    public (int CtScore, int TScore) LoadTeamsScore()
    {
        var teamEntities = Utilities.FindAllEntitiesByDesignerName<CCSTeam>("cs_team_manager");
        int ctScore = 0;
        int tScore = 0;
        foreach (var team in teamEntities)
        {
            if (team.Teamname.Equals("CT", StringComparison.OrdinalIgnoreCase))
            {
                ctScore = team.Score;
            }
            else if (team.Teamname.Equals("TERRORIST", StringComparison.OrdinalIgnoreCase))
            {
                tScore = team.Score;
            }
            else
            {
                _Logger.LogError("TeamName '{name}'is not supported!", team.Teamname);
            }
        }

        return (ctScore, tScore);
    }

    public void NextFrame(Action value)
    {
        CounterStrikeSharp.API.Server.NextFrame(value);
    }

    public void PrintToChatAll(string message)
    {
        CounterStrikeSharp.API.Server.PrintToChatAll(message);
    }

    // TODO Create Convar Object in _CsServer or as Service and wrap Properties explicit to get/set values
    public void UpdateConvar<T>(string name, T value)
    {
        try
        {
            if (value is string stringValue)
            {
                _Logger.LogInformation("Update ConVar {name} to stringvalue {value}", name, stringValue);
                ExecuteCommand($"{name} {stringValue}");
            }
            else
            {
                var convar = ConVar.Find(name);

                if (convar == null)
                {
                    _Logger.LogError("ConVar {name} couldn't be found", name);
                    return;
                }

                _Logger.LogInformation("Update ConVar {name} to value {value}", name, value);
                convar.SetValue(value);
            }
        }
        catch (Exception e)
        {
            _Logger.LogError(e, "Could not set cvar \"{name}\" to value \"{value}\" of type \"{type}\"", name, value, typeof(T).Name);
        }
    }

    public T? GetConvar<T>(string name)
    {
        var convar = ConVar.Find(name);
        if (convar == null)
        {
            _Logger.LogError("Convar {name} not found!", name);
            return default;
        }

        if (typeof(string).Equals(typeof(T)))
        {
            return (T)(object)convar.StringValue;
        }

        return convar.GetPrimitiveValue<T>();
    }

    public void DisableCheats()
    {
        _Logger.LogInformation("Disabling cheats");
        UpdateConvar("sv_cheats", value: false);
    }

    public void EndWarmup()
    {
        _Logger.LogInformation("Ending warmup immediately");
        ExecuteCommand("mp_warmup_end");
    }

    public void RestartGame()
    {
        _Logger.LogInformation("Restart Game");
        ExecuteCommand("mp_restartgame 1");
    }

    public void LoadAndExecuteConfig(string configFileName)
    {
        var absoluteConfigFilePath = Path.Combine(GameDirectory, "csgo", "cfg", "PugSharp", configFileName);
        if (!File.Exists(absoluteConfigFilePath))
        {
            _Logger.LogError("Config {configFile} was not found on the server.", absoluteConfigFilePath);
            return;
        }

        var configFilePath = $"PugSharp/{configFileName}";

        _Logger.LogTrace("Loading {configFilePath} with absolute path {absoluteConfigFilePath}.", configFilePath, absoluteConfigFilePath);
        ExecuteCommand($"exec {configFilePath}");
    }

    public void SetupRoundBackup(string prefix)
    {
        _Logger.LogInformation("Create round backup files: {prefix}", prefix);
        ExecuteCommand($"mp_backup_round_file {prefix}");
    }

    public string StartDemoRecording(string demoDirectory, string demoFileName)
    {
        try
        {
            if (!Directory.Exists(demoDirectory))
            {
                Directory.CreateDirectory(demoDirectory);
            }

            var relativeDemoDirectory = Path.GetRelativePath(Path.Combine(GameDirectory, "csgo"), demoDirectory);
            var relativeDemoFileName = Path.Join(relativeDemoDirectory, demoFileName);
            _Logger.LogInformation("Starting demo recording, path: {relativeDemoFileName}", relativeDemoFileName);
            ExecuteCommand($"tv_record {relativeDemoFileName}");
            var fullDemoPath = Path.Combine(demoDirectory, demoFileName + ".dem");
            _Logger.LogInformation("Started demo recording, path: \"{relativeDemoFileName}\", full path: \"{fullPath}\"", relativeDemoFileName, fullDemoPath);
            return fullDemoPath;
        }
        catch (Exception e)
        {
            _Logger.LogError(e, "Error Starting DemoRecording. Fallback to tv_record. Fallback to {demoFileName}", demoFileName);
            ExecuteCommand($"tv_record {demoFileName}");
        }

        return string.Empty;
    }

    public void StopDemoRecording()
    {
        _Logger.LogInformation("Stopping SourceTV demo recording");
        ExecuteCommand("tv_stoprecord");
    }


    public void RestoreBackup(string roundBackupFile)
    {
        _Logger.LogInformation("Restore Backup {backup}", roundBackupFile);
        ExecuteCommand($"mp_backup_restore_load_file {roundBackupFile}");
    }

    public void PauseMatch()
    {
        _Logger.LogInformation("Pausing the match in the next freeze time");
        ExecuteCommand("mp_pause_match");
    }

    public void UnpauseMatch()
    {
        _Logger.LogInformation("Resuming the match");
        ExecuteCommand("mp_unpause_match");
    }

    public void SwitchMap(string selectedMap)
    {
        if (!IsMapValid(selectedMap))
        {
            _Logger.LogInformation("The selected map is not valid: \"{selectedMap}\"!", selectedMap);
            return;
        }

        _Logger.LogInformation("Switch map to: \"{selectedMap}\"!", selectedMap);
        ExecuteCommand($"changelevel {selectedMap}");
    }

    public IReadOnlyList<IPlayer> LoadAllPlayers()
    {
        return Utilities.GetPlayers().Where(x => x.PlayerState() == PlayerConnectedState.PlayerConnected).Select(p => new Player(p.SteamID)).ToList();
    }

    public string CurrentMap => CounterStrikeSharp.API.Server.MapName;
}
