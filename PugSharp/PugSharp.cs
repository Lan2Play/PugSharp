using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using Microsoft.Extensions.Logging;
using PugSharp.ApiStats;
using PugSharp.Config;
using PugSharp.G5Api;
using PugSharp.Logging;
using PugSharp.Match.Contract;
using PugSharp.Models;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using ChatColors = CounterStrikeSharp.API.Modules.Utils.ChatColors;
using Player = PugSharp.Models.Player;

namespace PugSharp;

public class PugSharp : BasePlugin, IMatchCallback
{
    private static readonly ILogger<PugSharp> _Logger = LogManager.CreateLogger<PugSharp>();

    private readonly ConfigProvider _ConfigProvider = new(Path.Join(Server.GameDirectory, "csgo", "PugSharp", "Config"));

    private readonly CurrentRoundState _CurrentRountState = new CurrentRoundState();
    private ApiStats.ApiStats _ApiStats;
    private G5ApiClient _G5Stats;
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
        RegisterEventHandler<EventPlayerBlind>(OnPlayerBlind);
        RegisterEventHandler<EventRoundMvp>(OnRoundMvp);
        RegisterEventHandler<EventBombDefused>(OnBombDefused);
        RegisterEventHandler<EventBombPlanted>(OnBombPlanted);
        RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);

        RegisterListener<Listeners.OnMapStart>(OnMapStartHandler);

        AddCommandListener("jointeam", OnClientCommandJoinTeam);

        _Logger.LogInformation("End RegisterEventHandlers");
    }

    private void InitializeMatch(MatchConfig matchConfig)
    {
        var pluginDirectory = Path.Combine(Server.GameDirectory, "csgo", "PugSharp");

        _ApiStats = new ApiStats.ApiStats(matchConfig.EventulaApistatsUrl ?? string.Empty, matchConfig.EventulaApistatsToken ?? string.Empty, pluginDirectory == null ? null : Path.Combine(pluginDirectory, "Stats"));
        _G5Stats = new G5ApiClient(matchConfig.G5ApiUrl ?? string.Empty, matchConfig.G5ApiHeader ?? string.Empty, matchConfig.G5ApiHeaderValue ?? string.Empty);


        SetMatchVariable(matchConfig);

        _Match?.Dispose();
        _Match = new Match.Match(this, matchConfig, pluginDirectory);

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
        try
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

    public Task GoingLiveAsync(string matchId, string mapName, int mapNumber, CancellationToken cancellationToken)
    {
        return Task.WhenAll(
            _ApiStats.SendGoingLiveAsync(matchId, new GoingLiveParams(mapName, mapNumber), cancellationToken),
            _G5Stats.SendEventAsync(new GoingLiveEvent(matchId, mapNumber), cancellationToken));
    }


    public Task FinalizeMapAsync(string matchId, string winnerTeamName, int team1Score, int team2Score, int mapNumber, CancellationToken cancellationToken)
    {
        if (_ApiStats == null)
        {
            return Task.CompletedTask;
        }

        return Task.WhenAll(
            _ApiStats.FinalizeMapAsync(matchId, new MapResultParams(winnerTeamName, team1Score, team2Score, mapNumber), cancellationToken),
            _G5Stats.SendEventAsync(new MapResultEvent(matchId, mapNumber, new Winner((Side)(int)LoadMatchWinner(), team1Score > team2Score ? 1 : 2), null, null), cancellationToken));
    }

    public Task SendRoundStatsUpdateAsync(string matchId, int mapNumber, ITeamInfo team1Info, ITeamInfo team2Info, IMap currentMap, CancellationToken cancellationToken)
    {
        if (_ApiStats == null)
        {
            return Task.CompletedTask;
        }

        return _ApiStats.SendRoundStatsUpdateAsync(matchId, new RoundStatusUpdateParams(mapNumber, team1Info, team2Info, currentMap), cancellationToken);
    }

    public Task FinalizeAsync(string matchId, string winnerTeamName, bool forfeit, uint timeBeforeFreeingServerMs, int team1SeriesScore, int team2SeriesScore, CancellationToken cancellationToken)
    {
        if (_ApiStats == null)
        {
            return Task.CompletedTask;
        }

        return Task.WhenAll(
                 _ApiStats.FinalizeAsync(new SeriesResultParams(winnerTeamName, forfeit, timeBeforeFreeingServerMs), cancellationToken),
                 _G5Stats.SendEventAsync(new SeriesResultEvent(matchId, new Winner((Side)(int)LoadMatchWinner(), 0), team1SeriesScore, team2SeriesScore, (int)timeBeforeFreeingServerMs), cancellationToken));
    }

    public void CleanUpMatch()
    {
        if (_Match != null)
        {
            _Match.Dispose();
            _Match = null;
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
        //UpdateConvar("tv_delay", 30);
        //UpdateConvar("tv_delay1", 30);

        _Logger.LogInformation("Set match variables done");
    }

    #region Commands

    [ConsoleCommand("css_stopmatch", "Load a match config")]
    [ConsoleCommand("ps_stopmatch", "Load a match config")]
    public void OnCommandStopMatch(CCSPlayerController? player, CommandInfo command)
    {
        HandleCommand(() =>
        {
            if (player != null && !player.IsAdmin(_ServerConfig))
            {
                command.ReplyToCommand("Command is only allowed for admins!");
                return;
            }

            if (_Match == null)
            {
                command.ReplyToCommand("Currently no Match is running. ");
                return;
            }

            if (command.ArgCount < 2)
            {
                command.ReplyToCommand("Url is required as Argument!");
                return;
            }

            _Match.Dispose();
            _Match = null;
            ResetServer();
        },
        command);
    }

    [ConsoleCommand("css_loadconfig", "Load a match config")]
    [ConsoleCommand("ps_loadconfig", "Load a match config")]
    public void OnCommandLoadConfig(CCSPlayerController? player, CommandInfo command)
    {
        HandleCommand(() =>
        {
            if (player != null && !player.IsAdmin(_ServerConfig))
            {
                command.ReplyToCommand("Command is only allowed for admins!");
                return;
            }

            if (_Match != null)
            {
                command.ReplyToCommand("Currently Match {match} is running. To stop it call ps_stopmatch");
                return;
            }

            if (command.ArgCount < 2)
            {
                command.ReplyToCommand("Url is required as Argument!");
                return;
            }


            _Logger.LogInformation("Start loading match config!");

            var url = command.ArgByIndex(1);
            var authToken = command.ArgCount > 2 ? command.ArgByIndex(2) : string.Empty;

            command.ReplyToCommand($"Loading Config from {url}");
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
        },
        command);
    }

    [ConsoleCommand("css_loadconfigfile", "Load a match config from a file")]
    [ConsoleCommand("ps_loadconfigfile", "Load a match config from a file")]
    public void OnCommandLoadConfigFromFile(CCSPlayerController? player, CommandInfo command)
    {
        HandleCommand(() =>
        {
            if (player != null && !player.IsAdmin(_ServerConfig))
            {
                player.PrintToCenter("Command is only allowed for admins!");
                return;
            }

            if (_Match != null)
            {
                command.ReplyToCommand("Currently Match {match} is running. To stop it call ps_stopmatch");
                return;
            }

            if (command.ArgCount != 2)
            {
                _Logger.LogInformation("FileName is required as Argument! Path have to be put in \"pathToConfig\"");
                player?.PrintToCenter("FileName is required as Argument! Path have to be put in \"pathToConfig\"");

                return;
            }

            _Logger.LogInformation("Start loading match config!");
            var fileName = command.ArgByIndex(1);

            command.ReplyToCommand($"Loading Config from file {fileName}");
            var loadMatchConfigFromFileResult = _ConfigProvider.LoadMatchConfigFromFileAsync(fileName).Result;

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
        },
        command);
    }

    [ConsoleCommand("css_dumpmatch", "Serialize match to JSON on console")]
    [ConsoleCommand("ps_dumpmatch", "Load a match config")]
    public void OnCommandDumpMatch(CCSPlayerController? player, CommandInfo command)
    {
        HandleCommand(() =>
        {
            _Logger.LogInformation("################ dump match ################");
            _Logger.LogInformation(JsonSerializer.Serialize(_Match));
            _Logger.LogInformation("################ dump match ################");
        },
        command);
    }

    [ConsoleCommand("css_ready", "Mark player as ready")]
    public void OnCommandReady(CCSPlayerController? player, CommandInfo command)
    {
        HandleCommand(() =>
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
            if (!_Match.TryAddPlayer(matchPlayer))
            {
                _Logger.LogError("Can not toggle ready state. Player is not part of this match!");
                player.Kick();
                return;
            }

            // TODO Async Handling?
            _Match.TogglePlayerIsReadyAsync(matchPlayer).Wait();
        },
        command);
    }

    [ConsoleCommand("css_unpause", "Starts a match")]
    public void OnCommandUnpause(CCSPlayerController? player, CommandInfo command)
    {
        HandleCommand(() =>
        {
            if (player == null)
            {
                _Logger.LogInformation("Command unpause has been called by the server.");
                return;
            }

            _Match?.Unpause(new Player(player));
        },
        command);
    }

    [ConsoleCommand("css_pause", "Pauses the current match")]
    public void OnCommandPause(CCSPlayerController? player, CommandInfo command)
    {
        HandleCommand(() =>
        {
            if (player == null)
            {
                _Logger.LogInformation("Command Pause has been called by the server.");
                return;
            }

            _Match?.Pause(new Player(player));
        },
        command);
    }

    private static void HandleCommand(Action commandAction, CommandInfo command, [CallerMemberName] string? commandMethod = null)
    {
        var commandName = commandMethod?.Replace("OnCommand", "", StringComparison.OrdinalIgnoreCase);
        try
        {
            _Logger.LogInformation("Command \"{commandName}\" called.", commandName);
            commandAction();
        }
        catch (Exception e)
        {
            _Logger.LogError(e, "Error executing command {command}", commandName);
            command.ReplyToCommand($"Error executing command \"{commandName}\"!");
        }
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
                if (!_Match.PlayerBelongsToMatch(eventPlayerConnectFull.Userid.SteamID))
                {
                    _Logger.LogInformation("Player {playerName} does not belong to the match!", userId.PlayerName);

                    eventPlayerConnectFull.Userid.Kick();
                    return HookResult.Continue;
                }

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
                else
                {
                    _Logger.LogInformation("Match is is in state {state}. Connecting new players is not possible! Kick Player {player}!", _Match.CurrentState, userId.PlayerName);
                    userId.Kick();
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

        _CurrentRountState.Reset();

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
                },
                CTRoundResult = new TeamRoundResults
                {
                    Score = teamCT.Score,
                    ScoreT = isFirstHalf ? teamCT.ScoreSecondHalf : teamCT.ScoreFirstHalf,
                    ScoreCT = isFirstHalf ? teamCT.ScoreFirstHalf : teamCT.ScoreSecondHalf,
                },
                PlayerResults = CreatePlayerResults(),
            });

            // Toggle after last round in half
            if ((teamT.Score + teamCT.Score) == _Match.Config.MaxRounds / 2)
            {
                _Match.SwitchTeam();
            }
            // TODO OT handling
        }

        return HookResult.Continue;
    }


    private IReadOnlyDictionary<ulong, IPlayerRoundResults> CreatePlayerResults()
    {
        var dict = new Dictionary<ulong, IPlayerRoundResults>();

        foreach (var kvp in _CurrentRountState.PlayerStats)
        {
            dict[kvp.Key] = kvp.Value;
        }

        return dict;
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
        //_Match?.SwitchTeam();
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
            var victim = eventPlayerDeath.Userid;

            var victimSide = (TeamConstants)eventPlayerDeath.Userid.TeamNum;

            var attacker = eventPlayerDeath.Attacker;

            var attackerSide = eventPlayerDeath.Attacker?.TeamNum == null ? TeamConstants.TEAM_INVALID : (TeamConstants)eventPlayerDeath.Attacker.TeamNum;

            CCSPlayerController? assister = eventPlayerDeath.Assister;

            var killedByBomb = eventPlayerDeath.Weapon.Equals("planted_c4", StringComparison.OrdinalIgnoreCase);
            var isSuicide = (attacker == victim) && !killedByBomb;
            var isHeadshot = eventPlayerDeath.Headshot;
            var isClutcher = false; // TODO

            var victimStats = _CurrentRountState.GetPlayerRoundStats(victim.SteamID, victim.PlayerName);

            victimStats.Dead = true;

            if (!_CurrentRountState.FirstDeathDone)
            {
                _CurrentRountState.FirstDeathDone = true;

                switch (victimSide)
                {
                    case TeamConstants.TEAM_CT:
                        victimStats.FirstDeathCt = true;
                        break;
                    case TeamConstants.TEAM_T:
                        victimStats.FirstDeathT = true;
                        break;
                }
            }

            if (isSuicide)
            {
                victimStats.Suicide = true;
            }

            else if (!killedByBomb)
            {
                if (attacker != null)
                {
                    var attackerStats = _CurrentRountState.GetPlayerRoundStats(attacker.SteamID, attacker.PlayerName);

                    if (attackerSide == victimSide)
                    {
                        attackerStats.TeamKills++;
                    }
                    else
                    {
                        if (!_CurrentRountState.FirstKillDone)
                        {
                            _CurrentRountState.FirstKillDone = true;

                            switch (attackerSide)
                            {
                                case TeamConstants.TEAM_CT:
                                    attackerStats.FirstKillCt = true;
                                    break;
                                case TeamConstants.TEAM_T:
                                    attackerStats.FirstKillT = true;
                                    break;
                            }
                        }

                        attackerStats.Kills++;

                        if (isHeadshot)
                        {
                            attackerStats.HeadshotKills++;
                        }

                        if (isClutcher)
                        {
                            attackerStats.ClutchKills++;

                            switch (attackerSide)
                            {
                                case TeamConstants.TEAM_CT:
                                    _CurrentRountState.CounterTerroristsClutching = true;
                                    break;
                                case TeamConstants.TEAM_T:
                                    _CurrentRountState.TerroristsClutching = true;
                                    break;
                            }

                            var hasClutched = false; // TODO

                            if (hasClutched)
                            {
                                attackerStats.Clutched = true;

                                victimStats.ClutchKills = 0;
                            }
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
                }

                if (assister != null)
                {
                    var friendlyFire = attackerSide == victimSide;
                    var assistedFlash = eventPlayerDeath.Assistedflash;

                    // Assists should only count towards opposite team
                    if (!friendlyFire)
                    {
                        var assisterStats = _CurrentRountState.GetPlayerRoundStats(assister.SteamID, assister.PlayerName);

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
        return HookResult.Continue;
    }

    private HookResult OnPlayerBlind(EventPlayerBlind eventPlayerBlind, GameEventInfo info)
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

            var victim = eventPlayerBlind.Userid;

            var victimSide = eventPlayerBlind.Userid.TeamNum;

            var attacker = eventPlayerBlind.Attacker;

            var attackerSide = eventPlayerBlind.Attacker.TeamNum;

            if (attacker == victim)
            {
                return HookResult.Continue;
            }

            bool isFriendlyFire = victimSide == attackerSide;

            // 2.5 is an arbitrary value that closely matches the "enemies flashed" column of the in-game
            // scoreboard.
            if (eventPlayerBlind.BlindDuration >= 2.5)
            {
                var attackerStats = _CurrentRountState.GetPlayerRoundStats(attacker.SteamID, attacker.PlayerName);

                if (isFriendlyFire)
                {
                    attackerStats.FriendliesFlashed++;
                }
                else
                {
                    attackerStats.EnemiesFlashed++;
                }

            }
        }

        return HookResult.Continue;
    }

    private HookResult OnRoundMvp(EventRoundMvp eventRoundMvp, GameEventInfo info)
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
            var mvp = eventRoundMvp.Userid;
            var mvpStats = _CurrentRountState.GetPlayerRoundStats(mvp.SteamID, mvp.PlayerName);

            mvpStats.Mvp = true;
        }

        return HookResult.Continue;
    }

    private HookResult OnBombPlanted(EventBombPlanted eventBombPlanted, GameEventInfo info)
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
            var planter = eventBombPlanted.Userid;
            var planterStats = _CurrentRountState.GetPlayerRoundStats(planter.SteamID, planter.PlayerName);

            planterStats.BombPlanted = true;
        }

        return HookResult.Continue;
    }

    private HookResult OnPlayerHurt(EventPlayerHurt eventPlayerHurt, GameEventInfo info)
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
            var attacker = eventPlayerHurt.Attacker;

            if (attacker == null)
            {
                return HookResult.Continue;
            }

            var attackerSide = eventPlayerHurt.Attacker.TeamNum;

            var victim = eventPlayerHurt.Userid;

            var victimSide = eventPlayerHurt.Userid.TeamNum;

            var weapon = eventPlayerHurt.Weapon;

            var isUtility = CounterStrikeSharpExtensions.IsUtility(weapon);

            var attackerStats = _CurrentRountState.GetPlayerRoundStats(attacker.SteamID, attacker.PlayerName);

            var victimHealth = victim.Health;

            var damageCapped = eventPlayerHurt.DmgHealth > victimHealth ? victimHealth : eventPlayerHurt.DmgHealth;

            // Don't count friendlydamage
            if (attackerSide != victimSide)
            {
                attackerStats.Damage += damageCapped;

                if (isUtility)
                {
                    attackerStats.UtilityDamage += damageCapped;
                }
            }
        }

        return HookResult.Continue;
    }

    private HookResult OnBombDefused(EventBombDefused eventBombDefused, GameEventInfo info)
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
            var defuser = eventBombDefused.Userid;
            var defuserStats = _CurrentRountState.GetPlayerRoundStats(defuser.SteamID, defuser.PlayerName);

            defuserStats.BombDefused = true;
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

    public string StartDemoRecording()
    {
        if (_Match == null)
        {
            return string.Empty;
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
            return fullDemoFileName;
        }
        catch (Exception e)
        {
            _Logger.LogError(e, "Error Starting DemoRecording. Fallback to tv_record. Fallback to {demoFileName}", demoFileName);
            Server.ExecuteCommand($"tv_record {demoFileName}");
        }

        return string.Empty;
    }

    public void StopDemoRecording()
    {
        _Logger.LogInformation("Stopping SourceTV demo recording");
        Server.ExecuteCommand("tv_stoprecord");
    }

    public Match.Contract.Team LoadMatchWinner()
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