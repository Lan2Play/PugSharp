using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory;
using Microsoft.Extensions.Logging;
using PugSharp.Config;
using PugSharp.Logging;
using PugSharp.Match.Contract;
using System.Text.Json;

namespace PugSharp;



public class PugSharp : BasePlugin, IMatchCallback
{
    private static readonly ILogger<PugSharp> _Logger = LogManager.CreateLogger<PugSharp>();

    private readonly ConfigProvider _ConfigProvider = new();

    private Match.Match? _Match;

    public override string ModuleName => "PugSharp Plugin";

    public override string ModuleVersion => "0.0.1";


    public override void Load(bool hotReload)
    {
        _Logger.LogInformation("Loading PugSharp!");

        RegisterEventHandlers();
    }

    private void RegisterEventHandlers()
    {
        _Logger.LogInformation("Begin RegisterEventHandlers");

        RegisterEventHandler<EventCsWinPanelRound>(OnRoundWinPanel, HookMode.Pre);
        RegisterEventHandler<EventCsWinPanelMatch>(OnMatchOver);
        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerconnectFull);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        RegisterEventHandler<EventRoundEnd>(OnRoundEnd, HookMode.Pre);
        RegisterEventHandler<EventRoundFreezeEnd>(OnRoundFreezeEnd);
        RegisterEventHandler<EventRoundPrestart>(OnRoundPreStart);
        RegisterEventHandler<EventRoundPrestart>(OnRoundStart);
        RegisterEventHandler<EventServerCvar>(OnCvarChanged, HookMode.Pre);
        RegisterEventHandler<EventPlayerTeam>(OnPlayerTeam);

        _Logger.LogInformation("End RegisterEventHandlers");
    }

    private void ExecuteServerCommand(string command, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            Server.ExecuteCommand($"{command} {value}");
        }
    }

    public void InitializeMatch(MatchConfig matchConfig)
    {
        SetMatchVariable(matchConfig);

        _Match = new Match.Match(this, matchConfig);

        var players = GetAllPlayers();
        foreach (var player in players.Where(x => x.UserId.HasValue && x.UserId >= 0))
        {
            if (player.UserId != null && !_Match.TryAddPlayer(player))
            {
                KickPlayer(player.UserId.Value);
            }
        }
    }

    private void SetMatchVariable(MatchConfig matchConfig)
    {
        Server.ExecuteCommand("sv_disable_teamselect_menu true");
        Server.ExecuteCommand("sv_human_autojoin_team 2");
        Server.ExecuteCommand("mp_warmuptime 6000");

        Server.ExecuteCommand("mp_overtime_enable true");
        Server.ExecuteCommand("mp_overtime_maxrounds 6");
        Server.ExecuteCommand("mp_maxrounds 24");
        Server.ExecuteCommand("mp_tournament 1");

        Server.ExecuteCommand("mp_team_timeout_time 30");
        Server.ExecuteCommand("mp_team_timeout_max 3");


        ExecuteServerCommand($"mp_endmatch_votenextmap", "false");

        ExecuteServerCommand($"mp_teamname_1", matchConfig.Team1.Name);
        ExecuteServerCommand($"mp_teamflag_1", matchConfig.Team1.Flag);
        ExecuteServerCommand($"mp_teamname_2", matchConfig.Team2.Name);
        ExecuteServerCommand($"mp_teamflag_2", matchConfig.Team2.Flag);
    }

    #region Commands

    [ConsoleCommand("css_loadconfig", "Load a match config")]
    public void OnCommandLoadConfig(CCSPlayerController? player, CommandInfo command)
    {
        _Logger.LogInformation("Start loading match config!");
        if (command.ArgCount != 3)
        {
            _Logger.LogInformation("Url is required as Argument!");
            player?.PrintToCenter("Url is required as Argument!");

            return;

        }

        var url = command.ArgByIndex(1);
        var authToken = command.ArgByIndex(2);
        var result = _ConfigProvider.TryLoadConfigAsync(url, authToken).Result;
        if (result.Successful)
        {
            InitializeMatch(result.Config!);
        }
    }

    [ConsoleCommand("css_dumpmatch", "Serialize match to JSON on console")]
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

    [ConsoleCommand("css_start", "Starts a match")]
    public void OnCommandStart(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
        {
            _Logger.LogInformation("Command Start has been called by the server.");
            return;
        }

        _Logger.LogInformation("Start Command called.");
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
    }

    #endregion

    #region EventHandlers

    private HookResult OnPlayerconnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var userId = @event.Userid;

        if (userId != null && userId.IsValid)
        {
            // // Userid will give you a reference to a CCSPlayerController class
            _Logger.LogInformation($"Player {userId.PlayerName} has connected!");

            if (_Match != null && _Match.CurrentState == MatchState.WaitingForPlayersConnectedReady)
            {
                userId.PrintToChat($"Hello {userId.PlayerName}, welcome to match {_Match.Config.MatchId}");
                userId.PrintToChat($"type !ready to be marked as ready for the match");
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
        _Logger.LogInformation($"Player {userId.PlayerName} has disconnected!");

        _Match?.SetPlayerDisconnected(new Player(userId));

        return HookResult.Continue;
    }

    public HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
    {
        if (_Match != null)
        {
            var configTeam = _Match.GetPlayerTeam(@event.Userid.SteamID);

            if ((int)configTeam != @event.Team)
            {
                _Logger.LogInformation($"Player {@event.Userid.PlayerName} tried to join {@event.Team} but is not allowed!");
                var player = new Player(@event.Userid);

                Server.NextFrame(() =>
                {
                    player.SwitchTeam(configTeam);
                    player.MatchStats?.ResetStats();
                });
                return HookResult.Continue;

            }
        }
        else
        {
            var players = GetAllPlayers();
            foreach (var player in players.Where(x => x.UserId != null))
            {
                KickPlayer(player.UserId!.Value);
            }
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
        _Logger.LogInformation($"OnRoundStart called");

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
        _Logger.LogInformation($"OnRoundPreStart called");

        return HookResult.Continue;
    }

    private HookResult OnRoundFreezeEnd(EventRoundFreezeEnd @event, GameEventInfo info)
    {
        _Logger.LogInformation($"OnRoundFreezeEnd called");

        return HookResult.Continue;
    }

    private HookResult OnRoundEnd(EventRoundEnd eventRoundEnd, GameEventInfo info)
    {
        _Logger.LogInformation($"OnRoundEnd called");

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
        _Logger.LogInformation($"OnMatchOver called");

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
            // TODO Figure out who won

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
        _Logger.LogInformation($"On Round win panel");

        return HookResult.Continue;
    }

    #endregion

    private static void KickPlayer(int? userId)
    {
        if (userId == null)
        {
            return;
        }

        Server.ExecuteCommand($"kickid {userId} \"You are not part of the current match!\"");
    }

    #region Implementation of IMatchCallback

    public void SwitchMap(string selectedMap)
    {
        if (!Server.IsMapValid(selectedMap))
        {
            _Logger.LogInformation($"The selected map is not valid: \"{selectedMap}\"!");
            return;
        }

        Server.ExecuteCommand($"map {selectedMap}");

        if (_Match != null)
        {
            SetMatchVariable(_Match.Config);
        }
    }

    public IReadOnlyList<IPlayer> GetAllPlayers()
    {
        var playerEntities = Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller");

        return playerEntities.Where(x => PlayerState(x) == PlayerConnectedState.PlayerConnected).Select(p => new Player(p)).ToArray();
    }

    private static PlayerConnectedState PlayerState(CCSPlayerController player)
    {
        if (player == null || !player.IsValid)
        {
            return PlayerConnectedState.PlayerNeverConnected;
        }

        var statusRef = Schema.GetRef<UInt32>(player.Handle, "CBasePlayerController", "m_iConnected");

        return (PlayerConnectedState)statusRef;
    }

    public enum PlayerConnectionState
    {
        PlayerNeverConnected = 0xfffffff,
        PlayerConnected = 0x0,
        PlayerConnecting = 0x1,
        PlayerReconnecting = 0x2,
        PlayerDisconnecting = 0x3,
        PlayerDisconnected = 0x4,
        PlayerReserved = 0x5,
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
        // TODO Oder lieber mp_pause_match/mp_unpause_match sodass erst am ende der Runde eine pause gestartet wird?
        Server.ExecuteCommand("pause");
    }

    public void UnpauseServer()
    {
        Server.ExecuteCommand("unpause");
    }

    public void DisableCheats()
    {
        Server.ExecuteCommand("sv_cheats 0");
    }

    public void StartDemoRecording()
    {
        var demoFileName = $"PugSharp_Match_{_Match.Config.MatchId}_{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}";
        try
        {
            string directoryPath = Path.Join(Server.GameDirectory, "PugSharp", "Demo");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var fullDemoFileName = Path.Join(directoryPath, demoFileName);
            _Logger.LogInformation($"Starting demo recording, path: {fullDemoFileName}");
            Server.ExecuteCommand($"tv_record {fullDemoFileName}");
        }
        catch (Exception e)
        {
            _Logger.LogError(e, "Error Starting DemoRecording. Fallback to tv_record ");
            Server.ExecuteCommand($"tv_record {demoFileName}");
        }
    }

    public void StopDemoRecording()
    {
        Server.ExecuteCommand($"tv_stoprecord");
    }

    #endregion


    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _ConfigProvider.Dispose();
        }
    }

}