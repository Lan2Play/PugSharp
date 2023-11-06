using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using PugSharp.Config;
using PugSharp.Logging;
using PugSharp.Match.Contract;
using PugSharp.Models;
using System.Globalization;
using System.Text.Json;
using static System.Net.WebRequestMethods;

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

        var configPath = Path.Join(Server.GameDirectory, "csgo", "PugSharp", "Config", "server.json");
        var serverConfigResult = ConfigProvider.LoadServerConfig(configPath);

        serverConfigResult.Switch(
            error => { }, // Do nothing - Error already logged
            serverConfig => _ServerConfig = serverConfig
        );
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
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventServerCvar>(OnCvarChanged, HookMode.Pre);
        RegisterEventHandler<EventPlayerTeam>(OnPlayerTeam);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);

        RegisterListener<Listeners.OnMapStart>(OnMapStartHandler);

        AddCommandListener("jointeam", OnClientCommandJoinTeam);

        _Logger.LogInformation("End RegisterEventHandlers");
    }

    private void InitializeMatch(MatchConfig matchConfig)
    {
        SetMatchVariable(matchConfig);

        _Match?.Dispose();
        _Match = new Match.Match(this, matchConfig, Path.Combine(Server.GameDirectory, "csgo", "PugSharp"));

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

    public static void UpdateConvar<T>(string name, T value)
    {
        var convar = ConVar.Find(name);

        if (convar == null)
        {
            _Logger.LogError("ConVar {name} couldn't be found", name);
            return;
        }

        if (value is string stringValue)
        {
            convar.StringValue = stringValue;
        }
        else
        {
            convar.SetValue(value);
        }
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

        UpdateConvar("sv_disable_teamselect_menu", true);
        UpdateConvar("sv_human_autojoin_team", 2);
        UpdateConvar("mp_warmuptime", (float)6000);

        UpdateConvar("mp_overtime_enable", true);
        UpdateConvar("mp_overtime_maxrounds", matchConfig.MaxOvertimeRounds);
        UpdateConvar("mp_maxrounds", matchConfig.MaxRounds);
        //UpdateConvar("mp_tournament", true);
        UpdateConvar("mp_autokick", false);

        UpdateConvar("mp_team_timeout_time", 30);
        UpdateConvar("mp_team_timeout_max", 3);

        UpdateConvar("mp_competitive_endofmatch_extra_time", (float)120);
        //UpdateConvar("mp_chattime", (float)120);

        UpdateConvar("mp_endmatch_votenextmap", false);

        // Set T Name
        UpdateConvar("mp_teamname_1", matchConfig.Team2.Name);
        UpdateConvar("mp_teamflag_1", matchConfig.Team2.Flag);

        // Set CT Name
        UpdateConvar("mp_teamname_2", matchConfig.Team1.Name);
        UpdateConvar("mp_teamflag_2", matchConfig.Team1.Flag);

        UpdateConvar("tv_autorecord", false);

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
        if (command.ArgCount < 2)
        {
            _Logger.LogInformation("Url is required as Argument!");
            player?.PrintToCenter("Url is required as Argument!");

            return;
        }

        var url = command.ArgByIndex(1);
        var authToken = command.ArgCount > 2 ? command.ArgByIndex(2) : string.Empty;

        SendMessage($"Loading Config from {url}");
        var loadMatchConfigFromUrlResult = _ConfigProvider.LoadMatchConfigFromUrlAsync(url, authToken).Result;

        loadMatchConfigFromUrlResult.Switch(
            error =>
            {
                command.ReplyToCommand($"Loading config was not possible. Error: {error.Value}");
            },
            matchConfig =>
            {
                // Use same token for APIstats if theres no token set in the matchconfig
                if (string.IsNullOrEmpty(matchConfig.EventulaApistatsToken))
                {
                    matchConfig.EventulaApistatsToken = authToken;
                }

                command.ReplyToCommand("Matchconfig loaded!");

                InitializeMatch(matchConfig);
            }
        );
    }

    [ConsoleCommand("css_loadconfigfile", "Load a match config from a file")]
    [ConsoleCommand("ps_loadconfigfile", "Load a match config from a file")]
    public void OnCommandLoadConfigFromFile(CCSPlayerController? player, CommandInfo command)
    {
        if (player != null && !player.IsAdmin(_ServerConfig))
        {
            player.PrintToCenter("Command is only allowed for admins!");
            return;
        }

        _Logger.LogInformation("Start loading match config!");
        if (command.ArgCount != 2)
        {
            _Logger.LogInformation("FileName is required as Argument! Path have to be put in \"pathToConfig\"");
            player?.PrintToCenter("FileName is required as Argument! Path have to be put in \"pathToConfig\"");

            return;
        }

        var fileName = command.ArgByIndex(1);

        // TODO should it be relative to GameDirectory?
        var fullFilePath = Path.GetFullPath(fileName);

        SendMessage($"Loading Config from file {fullFilePath}");
        var loadMatchConfigFromFileResult = ConfigProvider.LoadMatchConfigFromFileAsync(fullFilePath).Result;

        loadMatchConfigFromFileResult.Switch(
            error =>
            {
                command.ReplyToCommand($"Loading config was not possible. Error: {error.Value}");
            },
            matchConfig =>
            {
                command.ReplyToCommand("Matchconfig loaded!");
                InitializeMatch(matchConfig);
            }
        );
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

    private HookResult OnPlayerConnectFull(EventPlayerConnectFull eventPlayerConnectFull, GameEventInfo info)
    {
        var userId = eventPlayerConnectFull.Userid;

        if (userId != null && userId.IsValid)
        {
            // // Userid will give you a reference to a CCSPlayerController class
            _Logger.LogInformation("Player {playerName} has connected!", userId.PlayerName);

            if (_Match != null)
            {
                if (_Match.CurrentState == MatchState.WaitingForPlayersConnectedReady)
                {
                    userId.PrintToChat($" {ChatColors.Default}Hello {ChatColors.Green}{userId.PlayerName}{ChatColors.Default}, welcome to match {_Match.Config.MatchId}");
                    userId.PrintToChat($" {ChatColors.Default}powered by {ChatColors.Green}PugSharp{ChatColors.Default} (https://github.com/Lan2Play/PugSharp/)");
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

    public HookResult OnPlayerDisconnect(EventPlayerDisconnect eventPlayerDisconnect, GameEventInfo info)
    {
        var userId = eventPlayerDisconnect.Userid;

        // Userid will give you a reference to a CCSPlayerController class
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
        if (_Match != null
            && _Match.CurrentState != MatchState.None
            && !eventCvarChanged.Cvarname.Equals("sv_cheats", StringComparison.OrdinalIgnoreCase))
        {
            info.DontBroadcast = true;
        }

        return HookResult.Continue;
    }

    private HookResult OnRoundStart(EventRoundStart eventRoundStart, GameEventInfo info)
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

    private HookResult OnRoundPreStart(EventRoundPrestart eventRoundPrestart, GameEventInfo info)
    {
        _Logger.LogInformation("OnRoundPreStart called");

        return HookResult.Continue;
    }

    private HookResult OnRoundFreezeEnd(EventRoundFreezeEnd eventRoundFreezeEnd, GameEventInfo info)
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
            var teamEntities = Utilities.FindAllEntitiesByDesignerName<CCSTeam>("cs_team_manager");

            var teamT = teamEntities.First(x => x.TeamNum == (int)Match.Contract.Team.Terrorist);
            var teamCT = teamEntities.First(x => x.TeamNum == (int)Match.Contract.Team.CounterTerrorist);

            var isFirstHalf = (teamT.Score + teamCT.Score) <= _Match.Config.MaxRounds / 2;
            _Match.SendRoundResults(new RoundResult
            {
                TRoundResult = new TeamRoundResults
                {
                    Score = teamT.Score,
                    ScoreT = isFirstHalf ? teamT.ScoreFirstHalf : teamT.ScoreSecondHalf,
                    ScoreCT = isFirstHalf ? teamT.ScoreSecondHalf : teamT.ScoreFirstHalf,
                    PlayerResults = CreatePlayerResults(teamT),
                },
                CTRoundResult = new TeamRoundResults
                {
                    Score = teamCT.Score,
                    ScoreT = isFirstHalf ? teamCT.ScoreSecondHalf : teamCT.ScoreFirstHalf,
                    ScoreCT = isFirstHalf ? teamCT.ScoreFirstHalf : teamCT.ScoreSecondHalf,
                    PlayerResults = CreatePlayerResults(teamCT),
                },
            });

            // TODO Reset round stats

            // TODO OT handling
        }

        return HookResult.Continue;
    }

    private IReadOnlyDictionary<ulong, IPlayerRoundResults> CreatePlayerResults(CCSTeam team)
    {
        var result = new Dictionary<ulong, IPlayerRoundResults>();

        var allPlayers = Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller");
        foreach (var playerController in team.PlayerControllers)
        {
            if (!playerController.IsValid)
            {
                _Logger.LogError("Can not create PlayerStatistics because controller is invalid!");
                continue;
            }

            var ccsPlayerController = allPlayers.First(x => x.SteamID.Equals(playerController.Value.SteamID));

            if (!ccsPlayerController.IsValid || ccsPlayerController.ActionTrackingServices == null)
            {
                _Logger.LogError("Can not create PlayerStatistics because controller is invalid!");
                continue;
            }

            var playerMatchStats = ccsPlayerController.ActionTrackingServices.MatchStats;

            // TODO should we store overall match stats in the PugSharp object or in the map object?
            // I think we should pass per round statistics (which are tracked in PugSharp class) to the Match object which holds the match wide statistics
            var internalPlayer = _Match?.AllMatchPlayers.FirstOrDefault(a => a.Player.UserId.Equals(ccsPlayerController.UserId));

            var stats = internalPlayer?.Player.MatchStats;

            // TODO Add Missing StatisticValues
            result.Add(playerController.Value.SteamID, new PlayerRoundResults
            {
                Assists = stats?.Assists ?? 0,
                BombDefuses = 0,
                BombPlants = 0,
                Coaching = false,
                ContributionScore = 0,
                Count1K = 0,
                Count2K = 0,
                Count3K = playerMatchStats.Enemy3Ks,
                Count4K = playerMatchStats.Enemy4Ks,
                Count5K = playerMatchStats.Enemy5Ks,
                Damage = playerMatchStats.Damage,
                Deaths = playerMatchStats.Deaths,
                EnemiesFlashed = playerMatchStats.EnemiesFlashed,
                FirstDeathCt = stats?.FirstDeathCt ?? 0,
                FirstDeathT = stats?.FirstDeathT ?? 0,
                FirstKillCt = stats?.FirstKillCt ?? 0,
                FirstKillT = stats?.FirstKillT ?? 0,
                FlashbangAssists = stats?.FlashbangAssists ?? 0,
                FriendliesFlashed = 0,
                HeadshotKills = stats?.HeadshotKills ?? 0,
                Kast = 0,
                Kills = playerMatchStats.Kills,
                KnifeKills = stats?.KnifeKills ?? 0,
                Mvp = 0,
                Name = playerController.Value.PlayerName,
                RoundsPlayed = 0,
                Suicides = stats?.Suicides ?? 0,
                TeamKills = stats?.TeamKills ?? 0,
                TradeKill = 0,
                UtilityDamage = playerMatchStats.UtilityDamage,
                V1 = 0,
                V2 = 0,
                V3 = 0,
                V4 = 0,
                V5 = 0,
            });
        }

        return result;
    }

    private HookResult OnPlayerSpawn(EventPlayerSpawn eventPlayerSpawn, GameEventInfo info)
    {
        if (_Match == null)
        {
            return HookResult.Continue;
        }

        if (_Match.CurrentState == MatchState.None)
        {
            return HookResult.Continue;
        }

        var userId = eventPlayerSpawn.Userid;
        if (_Match.CurrentState < MatchState.MatchRunning && userId != null && userId.IsValid)
        {
            // Give players max money if no match is running
            Server.NextFrame(() =>
            {
                var player = new Player(userId);

                int maxMoneyValue = 16000;

                // Use value from server if possible
                var maxMoneyCvar = ConVar.Find("mp_maxmoney");
                if (maxMoneyCvar != null)
                {

                    maxMoneyValue = maxMoneyCvar.GetPrimitiveValue<int>();
                }

                player.Money = maxMoneyValue;
            });
        }

        return HookResult.Continue;
    }

    private HookResult OnMatchOver(EventCsWinPanelMatch eventCsWinPanelMatch, GameEventInfo info)
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

    private HookResult OnEventRoundAnnounceLastRoundHalf(EventRoundAnnounceLastRoundHalf eventRoundAnnounceLastRoundHalf, GameEventInfo info)
    {
        _Logger.LogInformation("OnEventRoundAnnounceLastRoundHalf");
        _Match?.SwitchTeam();
        return HookResult.Continue;
    }

    private HookResult OnPlayerDeath(EventPlayerDeath eventPlayerDeath, GameEventInfo info)
    {
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
            var victim = _Match.AllMatchPlayers.FirstOrDefault(a => a.Player.UserId.Equals(eventPlayerDeath.Userid.UserId));

            var victimSide = eventPlayerDeath.Userid.TeamNum;

            var attacker = _Match.AllMatchPlayers.FirstOrDefault(a => a.Player.UserId.Equals(eventPlayerDeath.Attacker.UserId));

            var attackerSide = eventPlayerDeath.Attacker.TeamNum;

            Match.MatchPlayer? assister = null;

            if (eventPlayerDeath.Assister != null)
            {
                assister = _Match.AllMatchPlayers.FirstOrDefault(a => a.Player.UserId.Equals(eventPlayerDeath.Assister.UserId));
            }

            // TODO Update "clutch" (1vx) to check if the clutcher wins the round, this has to be stored somewhere per round

            var killedByBomb = eventPlayerDeath.Weapon.Equals("planted_c4");
            var isSuicide = (attacker == victim) && !killedByBomb;
            var isHeadshot = eventPlayerDeath.Headshot;

            var victimStats = victim.Player.MatchStats;

            var attackerStats = attacker?.Player.MatchStats;

            var assisterStats = assister?.Player.MatchStats;

            if (victimStats != null)
            {
                victimStats.Deaths++;
            }

            var isRoundFirstKillDone = false; // TODO this has to be stored somewhere per round

            var isRoundFirstDeathDone = false; // TODO this has to be stored somewhere per round

            if (!isRoundFirstDeathDone)
            {
                isRoundFirstDeathDone = true;

                if (victimStats != null)
                {
                    switch (victimSide)
                    {
                        // TODO Unknown values
                        case 1:
                            victimStats.FirstDeathCt++;
                            break;
                        case 2:
                            victimStats.FirstDeathT++;
                            break;
                    }
                }
            }

            if (isSuicide)
            {
                if (victimStats != null)
                {
                    victimStats.Suicides++;
                }
            }
            else if (!killedByBomb)
            {
                if (attackerSide == victimSide && attackerStats != null)
                {
                    attackerStats.TeamKills++;
                }
                else
                {
                    if (!isRoundFirstKillDone)
                    {
                        isRoundFirstKillDone = true;

                        if (attackerStats != null)
                        {
                            switch (attackerSide)
                            {
                                // TODO Unknown values
                                case 1:
                                    attackerStats.FirstKillCt++;
                                    break;
                                case 2:
                                    attackerStats.FirstKillT++;
                                    break;
                            }
                        }
                    }

                    if (attackerStats != null)
                    {
                        attackerStats.Kills++;

                        if (isHeadshot)
                        {
                            attackerStats.HeadshotKills++;
                        }
                        var weaponId = Enum.Parse<CSWeaponID>(eventPlayerDeath.WeaponItemid);

                        // Other than these constants, all knives can be found after CSWeapon_MAX_WEAPONS_NO_KNIFES.
                        // See https://sourcemod.dev/#/cstrike/enumeration.CSWeaponID
                        if (weaponId == CSWeaponID.KNIFE || weaponId == CSWeaponID.KNIFE_GG || weaponId == CSWeaponID.KNIFE_T ||
                            weaponId == CSWeaponID.KNIFE_GHOST || weaponId > CSWeaponID.MAX_WEAPONS_NO_KNIFES)
                        {
                            attackerStats.KnifeKills++;
                        }
                    }

                    if (assister != null)
                    {
                        bool friendlyFire = attackerSide == victimSide;
                        bool assistedFlash = eventPlayerDeath.Assistedflash;

                        // Assists should only count towards opposite team
                        if (!friendlyFire)
                        {
                            // You cannot flash-assist and regular-assist for the same kill.
                            if (assistedFlash)
                            {
                                if (assisterStats != null)
                                {
                                    assisterStats.FlashbangAssists++;
                                }
                            }
                            else
                            {
                                if (assisterStats != null)
                                {
                                    assisterStats.Assists++;
                                }
                            }

                        }
                    }
                }
            }
        }
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
        _Logger.LogInformation("Ending warmup immediately");
        Server.ExecuteCommand("mp_warmup_end");
    }

    public void PauseMatch()
    {
        _Logger.LogInformation("Pausing the match in the next freeze time");
        Server.ExecuteCommand("mp_pause_match");
    }

    public void UnpauseMatch()
    {
        _Logger.LogInformation("Resuming the match");
        Server.ExecuteCommand("mp_unpause_match");
    }

    public void DisableCheats()
    {
        _Logger.LogInformation("Disabling cheats");
        UpdateConvar("sv_cheats", false);
    }

    public void SetupRoundBackup()
    {
        var prefix = $"PugSharp_{_Match?.Config.MatchId}_";
        _Logger.LogInformation("Create round backup files: {prefix}", prefix);
        UpdateConvar("mp_backup_round_file", prefix);
    }

    public void StartDemoRecording()
    {
        if (_Match == null)
        {
            return;
        }

        var formattedDateTime = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        var demoFileName = $"PugSharp_Match_{_Match.Config.MatchId}_{formattedDateTime}.dem";
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
        _Logger.LogInformation("Stopping SourceTV demo recording");
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