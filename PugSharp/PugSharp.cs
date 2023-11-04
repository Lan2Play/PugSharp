using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using PugSharp.Config;
using PugSharp.Logging;
using PugSharp.Match.Contract;
using System.Globalization;
using System.Text.Json;

namespace PugSharp;

public class PugSharp : BasePlugin, IMatchCallback
{
    private static readonly ILogger<PugSharp> _Logger = LogManager.CreateLogger<PugSharp>();

    private readonly ConfigProvider _ConfigProvider = new();

    private Match.Match? _Match;
    private ServerConfig? _ServerConfig;

    public override string ModuleName => "PugSharp Plugin";

    public override string ModuleVersion => "0.0.1";


    public override void Load(bool hotReload)
    {
        _Logger.LogInformation("Loading PugSharp!");
        RegisterEventHandlers();
        _ = Task.Run(async () =>
        {
            var configPath = Path.Join(Server.GameDirectory, "PugSharp", "Config", "server.json");
            var serverConfigResult = await _ConfigProvider.LoadServerConfigAsync(configPath).ConfigureAwait(false);
            if (serverConfigResult.Successful)
            {
                _ServerConfig = serverConfigResult.Config;
            }
        });
    }

    private void RegisterEventHandlers()
    {
        _Logger.LogInformation("Begin RegisterEventHandlers");

        RegisterEventHandler<EventCsWinPanelRound>(OnRoundWinPanel, HookMode.Pre);
        RegisterEventHandler<EventCsWinPanelMatch>(OnMatchOver);
        RegisterEventHandler<EventRoundAnnounceLastRoundHalf>(OnEventRoundAnnounceLastRoundHalf);
        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        RegisterEventHandler<EventRoundEnd>(OnRoundEnd, HookMode.Pre);
        RegisterEventHandler<EventRoundFreezeEnd>(OnRoundFreezeEnd);
        RegisterEventHandler<EventRoundPrestart>(OnRoundPreStart);
        RegisterEventHandler<EventRoundPrestart>(OnRoundStart);
        RegisterEventHandler<EventServerCvar>(OnCvarChanged, HookMode.Pre);
        RegisterEventHandler<EventPlayerTeam>(OnPlayerTeam);

        RegisterListener<CounterStrikeSharp.API.Core.Listeners.OnMapStart>(OnMapStartHandler);

        AddCommandListener("jointeam", OnClientCommandJoinTeam);

        _Logger.LogInformation("End RegisterEventHandlers");
    }

    private static void ExecuteServerCommand(string command, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            Server.ExecuteCommand($"{command} {value}");
        }
    }

    private void InitializeMatch(MatchConfig matchConfig)
    {
        SetMatchVariable(matchConfig);

        _Match?.Dispose();
        _Match = new Match.Match(this, matchConfig);

        var players = GetAllPlayers();
        foreach (var player in players.Where(x => x.UserId.HasValue && x.UserId >= 0))
        {
            if (player.UserId != null && !_Match.TryAddPlayer(player))
            {
                player.Kick();
            }
        }

        ResetServer();
    }

    private void ResetServer()
    {
        StopDemoRecording();

        // TODO Configure VoteMap/or reload current map
        Server.ExecuteCommand("changelevel de_dust2");
    }

    private void SetMatchVariable(MatchConfig matchConfig)
    {
        _Logger.LogInformation("Start set match variables");

        ExecuteServerCommand("sv_disable_teamselect_menu", "true");
        ExecuteServerCommand("sv_human_autojoin_team", "2");
        ExecuteServerCommand("mp_warmuptime", "6000");

        ExecuteServerCommand("mp_overtime_enable", "true");
        ExecuteServerCommand("mp_overtime_maxrounds", matchConfig.MaxOvertimeRounds.ToString(CultureInfo.InvariantCulture));
        ExecuteServerCommand("mp_maxrounds", matchConfig.MaxRounds.ToString(CultureInfo.InvariantCulture));
        ExecuteServerCommand("mp_tournament", "1");
        ExecuteServerCommand("mp_autokick", "0");

        ExecuteServerCommand("mp_team_timeout_time", "30");
        ExecuteServerCommand("mp_team_timeout_max", "3");

        ExecuteServerCommand("mp_competitive_endofmatch_extra_time", "120");
        ExecuteServerCommand("mp_chattime", "120");

        ExecuteServerCommand("mp_endmatch_votenextmap", "false");

        ExecuteServerCommand("mp_teamname_1", matchConfig.Team1.Name);
        ExecuteServerCommand("mp_teamflag_1", matchConfig.Team1.Flag);
        ExecuteServerCommand("mp_teamname_2", matchConfig.Team2.Name);
        ExecuteServerCommand("mp_teamflag_2", matchConfig.Team2.Flag);

        ExecuteServerCommand("tv_autorecord", "0");

        _Logger.LogInformation("Set match variables done");
    }

    #region Commands

    [ConsoleCommand("css_loadconfig", "Load a match config")]
    [ConsoleCommand("ps_loadconfig", "Load a match config")]
    public void OnCommandLoadConfig(CCSPlayerController? player, CommandInfo command)
    {
        if (player != null && !player.IsAdmin(_ServerConfig))
        {
            player.PrintToCenter("Command is only allowed for admins!");
            return;
        }

        _Logger.LogInformation("Start loading match config!");
        if (command.ArgCount != 3)
        {
            _Logger.LogInformation("Url is required as Argument!");
            player?.PrintToCenter("Url is required as Argument!");

            return;
        }

        var url = command.ArgByIndex(1);
        var authToken = command.ArgByIndex(2);

        SendMessage($"Loading Config from {url}");
        var result = _ConfigProvider.TryLoadConfigAsync(url, authToken).Result;
        if (result.Successful)
        {
            if (string.IsNullOrEmpty(result.Config!.EventulaApistatsToken))
            {
                result.Config!.EventulaApistatsToken = authToken;
            }

            Server.PrintToConsole("Matchconfig loaded!");

            InitializeMatch(result.Config!);
        }
    }

    [ConsoleCommand("css_dumpmatch", "Serialize match to JSON on console")]
    [ConsoleCommand("ps_dumpmatch", "Load a match config")]
    public void OnCommandDumpMatch(CCSPlayerController? player, CommandInfo command)
    {
        _Logger.LogInformation("################ dump match ################");
        _Logger.LogInformation(JsonSerializer.Serialize(_Match));
        _Logger.LogInformation("################ dump match ################");
    }

    [ConsoleCommand("css_ready", "Mark player as ready")]
    public void OnCommandReady(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
        {
            _Logger.LogInformation("Command Start has been called by the server. Player is required to be marked as ready");
            return;
        }

        if (_Match == null)
        {
            return;
        }

        var matchPlayer = new Player(player);
        _Match.TryAddPlayer(matchPlayer);
        _ = _Match.TogglePlayerIsReadyAsync(matchPlayer);

        _Logger.LogInformation("Command ready called.");
    }

    [ConsoleCommand("css_unpause", "Starts a match")]
    public void OnCommandUnpause(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
        {
            _Logger.LogInformation("Command unpause has been called by the server.");
            return;
        }

        _Logger.LogInformation("Unpause Command called.");
        _Match?.Unpause(new Player(player));
    }

    [ConsoleCommand("css_pause", "Pauses the current match")]
    public void OnCommandPause(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
        {
            _Logger.LogInformation("Command Pause has been called by the server.");
            return;
        }

        _Logger.LogInformation("Pause Command called.");
        _Match?.Pause(new Player(player));
    }

    #endregion

    #region EventHandlers

    private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var userId = @event.Userid;

        if (userId != null && userId.IsValid)
        {
            // // Userid will give you a reference to a CCSPlayerController class
            _Logger.LogInformation("Player {playerName} has connected!", userId.PlayerName);

            if (_Match != null)
            {
                if (_Match.CurrentState == MatchState.WaitingForPlayersConnectedReady)
                {
                    userId.PrintToChat($" {ChatColors.Default}Hello {ChatColors.Green}{userId.PlayerName}{ChatColors.Default}, welcome to match {_Match.Config.MatchId}");
                    userId.PrintToChat($" {ChatColors.Default}type {ChatColors.BlueGrey}!ready {ChatColors.Default}to be marked as ready for the match");
                }
                else if (_Match.CurrentState == MatchState.MatchPaused)
                {
                    _Match.TryAddPlayer(new Player(userId));
                }
            }
            else if (!userId.IsAdmin(_ServerConfig))
            {
                _Logger.LogInformation("No match is loaded. Kick Player {player}!", userId.PlayerName);
                userId.Kick();
            }
        }
        else
        {
            _Logger.LogInformation($"Ivalid Player has connected!");
        }

        return HookResult.Continue;
    }

    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var userId = @event.Userid;

        // // Userid will give you a reference to a CCSPlayerController class
        _Logger.LogInformation("Player {playerName} has disconnected!", userId.PlayerName);

        _Match?.SetPlayerDisconnected(new Player(userId));

        return HookResult.Continue;
    }

    public HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
    {
        _Logger.LogInformation("OnPlayerTeam called");
        if (_Match != null)
        {
            var configTeam = _Match.GetPlayerTeam(@event.Userid.SteamID);

            if ((int)configTeam != @event.Team)
            {
                _Logger.LogInformation("Player {playerName} tried to join {team} but is not allowed!", @event.Userid.PlayerName, @event.Team);
                var player = new Player(@event.Userid);

                Server.NextFrame(() =>
                {
                    _Logger.LogInformation("Switch {playerName} to team {team}!", player.PlayerName, configTeam);
                    player.SwitchTeam(configTeam);
                    player.MatchStats?.ResetStats();
                });
            }
        }
        else if (!@event.Userid.IsAdmin(_ServerConfig))
        {
            _Logger.LogInformation("No match is loaded. Kick Player {player}!", @event.Userid.PlayerName);
            @event.Userid.Kick();
        }

        return HookResult.Continue;
    }

    private HookResult OnCvarChanged(EventServerCvar eventCvarChanged, GameEventInfo info)
    {
        if (_Match != null && _Match.CurrentState != MatchState.None)
        {
            // Silences cvar changes when executing live/knife/warmup configs, *unless* it's sv_cheats.
            if (!eventCvarChanged.Cvarname.Equals("sv_cheats"))
            {
                info.DontBroadcast = true;
            }
        }

        return HookResult.Continue;
    }

    private HookResult OnRoundStart(EventRoundPrestart @event, GameEventInfo info)
    {
        _Logger.LogInformation("OnRoundStart called");

        if (_Match == null)
        {
            return HookResult.Continue;
        }

        if (_Match.CurrentState == MatchState.None)
        {
            return HookResult.Continue;
        }

        // TODO Write Backup file

        return HookResult.Continue;
    }

    private HookResult OnRoundPreStart(EventRoundPrestart @event, GameEventInfo info)
    {
        _Logger.LogInformation("OnRoundPreStart called");

        return HookResult.Continue;
    }

    private HookResult OnRoundFreezeEnd(EventRoundFreezeEnd @event, GameEventInfo info)
    {
        _Logger.LogInformation("OnRoundFreezeEnd called");

        return HookResult.Continue;
    }

    private HookResult OnRoundEnd(EventRoundEnd eventRoundEnd, GameEventInfo info)
    {
        _Logger.LogInformation("OnRoundEnd called");

        if (_Match == null)
        {
            return HookResult.Continue;
        }

        if (_Match.CurrentState == MatchState.None)
        {
            return HookResult.Continue;
        }

        if (_Match.CurrentState == MatchState.MatchRunning)
        {
            // TODO Update stats

            // TODO OT handling
        }

        return HookResult.Continue;
    }

    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        if (_Match == null)
        {
            return HookResult.Continue;
        }

        if (_Match.CurrentState == MatchState.None)
        {
            return HookResult.Continue;
        }

        var userId = @event.Userid;
        if (_Match.CurrentState < MatchState.MatchRunning && userId != null && userId.IsValid)
        {
            // Give players max money if no match is running
            Server.NextFrame(() =>
            {
                var player = new Player(userId);

                // TODO read mp_maxmoney cvar
                player.Money = 16000;
            });
        }

        return HookResult.Continue;
    }

    private HookResult OnMatchOver(EventCsWinPanelMatch @event, GameEventInfo info)
    {
        _Logger.LogInformation("OnMatchOver called");

        if (_Match == null)
        {
            return HookResult.Continue;
        }

        if (_Match.CurrentState == MatchState.None)
        {
            return HookResult.Continue;
        }

        // TODO wait for GOTV recording to finish

        if (_Match.CurrentState == MatchState.MatchRunning)
        {
            var scores = LoadTeamsScore();
            _Match?.CompleteMap(scores.TScore, scores.CtScore);


            // TODO Figure out who won => Done In match Complete

            // TODO Update stats

            // TODO Fire map result event

            // TODO If we use series functionality check if the series is over

            // TODO Fire series event

            // TODO Reset server to defaults
        }
        return HookResult.Continue;
    }

    private HookResult OnRoundWinPanel(EventCsWinPanelRound eventCsWinPanelRound, GameEventInfo info)
    {
        _Logger.LogInformation("On Round win panel");
        return HookResult.Continue;
    }

    private HookResult OnEventRoundAnnounceLastRoundHalf(EventRoundAnnounceLastRoundHalf @event, GameEventInfo info)
    {
        _Logger.LogInformation("OnEventRoundAnnounceLastRoundHalf");
        _Match?.SwitchTeam();
        return HookResult.Continue;
    }

    #endregion

    #region Listeners

    private void OnMapStartHandler(string mapName)
    {
        if (_Match != null)
        {
            Server.NextFrame(() =>
            {
                SetMatchVariable(_Match.Config);
            });
        }
    }

    #endregion

    #region ClientCommandListener

    private HookResult OnClientCommandJoinTeam(CCSPlayerController? player, CommandInfo commandInfo)
    {
        _Logger.LogInformation("OnClientCommandJoinTeam was called!");
        if (player != null && player.IsValid)
        {
            _Logger.LogInformation("Player {playerName} tried to switch team!", player.PlayerName);
        }

        return HookResult.Stop;
    }

    #endregion

    #region Implementation of IMatchCallback

    public void SwitchMap(string selectedMap)
    {
        if (!Server.IsMapValid(selectedMap))
        {
            _Logger.LogInformation("The selected map is not valid: \"{selectedMap}\"!", selectedMap);
            return;
        }

        _Logger.LogInformation("Switch map to: \"{selectedMap}\"!", selectedMap);
        Server.ExecuteCommand($"changelevel {selectedMap}");
    }

    public void SwapTeams()
    {
        _Logger.LogInformation("Swap Teams");
        Server.ExecuteCommand("mp_swapteams");
    }

    public IReadOnlyList<IPlayer> GetAllPlayers()
    {
        var playerEntities = Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller");

        return playerEntities.Where(x => x.PlayerState() == PlayerConnectedState.PlayerConnected).Select(p => new Player(p)).ToArray();
    }

    public IReadOnlyCollection<string> GetAvailableMaps()
    {
        return Server.GetMapList().ToList();
    }

    public void SendMessage(string message)
    {
        Server.PrintToChatAll(message);
    }

    public void EndWarmup()
    {
        Server.ExecuteCommand("mp_warmup_end");
    }

    public void PauseServer()
    {
        Server.ExecuteCommand("mp_pause_match");
    }

    public void UnpauseServer()
    {
        Server.ExecuteCommand("mp_unpause_match");
    }

    public void DisableCheats()
    {
        _Logger.LogInformation("Disable cheats");
        Server.ExecuteCommand("sv_cheats 0");
    }

    public void SetupRoundBackup()
    {
        var prefix = $"PugSharp_{_Match?.Config.MatchId}_";
        _Logger.LogInformation("Create round backup files: {prefix}", prefix);
        Server.ExecuteCommand($"mp_backup_round_file {prefix}");
    }

    public void StartDemoRecording()
    {
        if (_Match == null)
        {
            return;
        }

        var demoFileName = $"PugSharp_Match_{_Match.Config.MatchId}_{DateTime.UtcNow:yyyyMMddHHmmss}.dem";
        try
        {
            string directoryPath = Path.Join(Server.GameDirectory, "csgo", "Demo");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var fullDemoFileName = Path.Join(directoryPath, demoFileName);
            _Logger.LogInformation("Starting demo recording, path: {fullDemoFileName}", fullDemoFileName);
            Server.ExecuteCommand($"tv_record {fullDemoFileName}");
        }
        catch (Exception e)
        {
            _Logger.LogError(e, "Error Starting DemoRecording. Fallback to tv_record. Fallback to {demoFileName}", demoFileName);
            Server.ExecuteCommand($"tv_record {demoFileName}");
        }
    }

    public void StopDemoRecording()
    {
        Server.ExecuteCommand("tv_stoprecord");
    }

    public Match.Contract.Team LoadMatchWinnerName()
    {
        var (CtScore, TScore) = LoadTeamsScore();
        if (CtScore > TScore)
        {
            return Match.Contract.Team.CounterTerrorist;
        }

        if (TScore > CtScore)
        {
            return Match.Contract.Team.Terrorist;
        }

        return Match.Contract.Team.None;
    }

    private static (int CtScore, int TScore) LoadTeamsScore()
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
        }

        return (ctScore, tScore);
    }

    #endregion

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _Match?.Dispose();
            _ConfigProvider.Dispose();
        }
    }

}