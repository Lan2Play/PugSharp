using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PugSharp.Api.Contract;
using PugSharp.Api.Json;
using PugSharp.Config;
using PugSharp.Match;
using PugSharp.Match.Contract;
using PugSharp.Models;
using PugSharp.Server.Contract;
using PugSharp.Shared;
using PugSharp.Translation;
using PugSharp.Translation.Properties;

namespace PugSharp;
public class Application : IApplication
{
    private const int _SwitchPlayerDelay = 1000;
    private readonly ILogger<Application> _Logger;
    private readonly IBasePlugin _Plugin;
    private readonly ICsServer _CsServer;
    private readonly MultiApiProvider _ApiProvider;
    private readonly G5CommandProvider _G5CommandProvider;
    private readonly ITextHelper _TextHelper;
    private readonly IServiceProvider _ServiceProvider;
    private readonly ConfigProvider _ConfigProvider;
    private readonly PeriodicTimer _ConfigTimer = new(TimeSpan.FromSeconds(1));
    private readonly CancellationTokenSource _CancellationTokenSource = new();

    public string PugSharpDirectory { get; }

    private Match.Match? _Match;
    private bool _DisposedValue;
    private ConfigCreator _ConfigCreator;
    private readonly CurrentRoundState _CurrentRountState = new();

    /// <summary>
    /// Create instance of Application
    /// </summary>
#pragma warning disable S107 // Methods should not have too many parameters
    public Application(
        ILogger<Application> logger,
        IBasePlugin plugin,
        ICsServer csServer,
        MultiApiProvider apiProvider,
        G5CommandProvider g5CommandProvider,
        ITextHelper textHelper,
        IServiceProvider serviceProvider,
        ConfigProvider configProvider)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        _Logger = logger;
        _Plugin = plugin;
        _CsServer = csServer;
        _ApiProvider = apiProvider;
        _G5CommandProvider = g5CommandProvider;
        _TextHelper = textHelper;
        _ServiceProvider = serviceProvider;
        _ConfigProvider = configProvider;

        PugSharpDirectory = Path.Combine(_CsServer.GameDirectory, "csgo", "PugSharp");

        _ = Task.Run(ConfigLoaderTask, _CancellationTokenSource.Token);
    }

    public void Initialize(bool hotReload)
    {
        RegisterEventHandlers();

        _Plugin.RegisterConsoleCommandAttributeHandlers(this);

        if (!hotReload)
        {
            var commands = _G5CommandProvider.LoadProviderCommands();
            foreach (var command in commands)
            {
                _Plugin.AddCommand(command.Name, command.Description, (p, c) =>
                {
                    HandleCommand(() =>
                    {
                        var args = Enumerable.Range(0, c.ArgCount).Select(i => c.GetArg(i)).ToArray();
                        var results = command.CommandCallBack(args);
                        foreach (var result in results)
                        {
                            c.ReplyToCommand(result);
                        }
                    }, c, player: null, c.GetCommandString, c.ArgString);
                });
            }
        }
    }

    private void RegisterEventHandlers()
    {
        _Logger.LogInformation("Begin RegisterEventHandlers");

        _Plugin.RegisterEventHandler<EventCsWinPanelMatch>(OnMatchOver);
        _Plugin.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        _Plugin.RegisterEventHandler<EventPlayerTeam>(OnPlayerTeam);
        _Plugin.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        _Plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);

        _Plugin.RegisterListener<Listeners.OnMapStart>(OnMapStartHandler);
        _Plugin.RegisterEventHandler<EventRoundPrestart>(OnRoundPreStart);
        _Plugin.RegisterEventHandler<EventRoundStart>(OnRoundStart);
        _Plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd, HookMode.Pre);
        _Plugin.RegisterEventHandler<EventRoundMvp>(OnRoundMvp);

        _Plugin.RegisterEventHandler<EventServerCvar>(OnCvarChanged, HookMode.Pre);

        _Plugin.RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);
        _Plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        _Plugin.RegisterEventHandler<EventPlayerBlind>(OnPlayerBlind);
        _Plugin.RegisterEventHandler<EventBombDefused>(OnBombDefused);
        _Plugin.RegisterEventHandler<EventBombPlanted>(OnBombPlanted);

        _Plugin.AddCommandListener("jointeam", OnClientCommandJoinTeam);

        _Logger.LogInformation("End RegisterEventHandlers");
    }

    #region EventHandler

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

        if (_Match.CurrentState == MatchState.MatchRunning)
        {
            var (CtScore, TScore) = _CsServer.LoadTeamsScore();
            _Match.CompleteMap(TScore, CtScore);
        }
        return HookResult.Continue;
    }


    private HookResult OnPlayerConnectFull(EventPlayerConnectFull eventPlayerConnectFull, GameEventInfo info)
    {
        var userId = eventPlayerConnectFull.Userid;

        if (userId != null && userId.IsValid)
        {
            // // Userid will give you a reference to a CCSPlayerController class
            _Logger.LogInformation("Player {playerName} has connected!", userId.PlayerName);

            if (userId.IsHLTV)
            {
                return HookResult.Continue;
            }

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
                    userId.PrintToChat(_TextHelper.GetText(nameof(Resources.PugSharp_Hello), userId.PlayerName, _Match.MatchInfo.Config.MatchId));
                    userId.PrintToChat(_TextHelper.GetText(nameof(Resources.PugSharp_PoweredBy)));
                    userId.PrintToChat(_TextHelper.GetText(nameof(Resources.PugSharp_NotifyReady)));
                }
                else if (_Match.CurrentState == MatchState.MatchPaused
                      || _Match.CurrentState == MatchState.RestoreMatch)
                {
                    _Match.TryAddPlayer(new Player(userId));
                }
                else
                {
                    // do nothing
                }
            }
            else
            {
                // Do nothign if no match is loaded
            }
        }
        else
        {
            _Logger.LogInformation($"Ivalid Player has connected!");
        }

        return HookResult.Continue;
    }

    public HookResult OnPlayerTeam(EventPlayerTeam eventPlayerTeam, GameEventInfo info)
    {
        _Logger.LogInformation("OnPlayerTeam called. {playerName} tries to join {team} IsHLTV: {isHLTV} IsBot {isBot}", eventPlayerTeam.Userid.PlayerName, eventPlayerTeam.Team, eventPlayerTeam.Userid.IsHLTV, eventPlayerTeam.Userid.IsBot);

        // Ignore SourceTV
        if (eventPlayerTeam.Userid.IsHLTV)
        {
            return HookResult.Continue;
        }

        if (_Match != null)
        {
            CheckMatchPlayerTeam(eventPlayerTeam.Userid, eventPlayerTeam.Team);
        }
        else
        {
            // Do nothing
        }

        return HookResult.Continue;
    }

    private async void CheckMatchPlayerTeam(CCSPlayerController playerController, int team)
    {
        if (_Match == null || !playerController.IsValid)
        {
            return;
        }

        if (_Match.CurrentState == MatchState.WaitingForPlayersConnectedReady || _Match.CurrentState == MatchState.WaitingForPlayersReady)
        {
            var configTeam = _Match.GetPlayerTeam(playerController.SteamID);
            var localPlayer = playerController;

            if ((int)configTeam != team)
            {
                await Task.Delay(_SwitchPlayerDelay, _CancellationTokenSource.Token).ConfigureAwait(false);

                _Logger.LogInformation("Player {playerName} tried to join {team} but should be in {configTeam}!", localPlayer.PlayerName, team, configTeam);
                var player = new Player(localPlayer);

                _Logger.LogInformation("Switch {playerName} to team {team}!", player.PlayerName, configTeam);
                player.SwitchTeam(configTeam);
            }
        }
    }

    public HookResult OnPlayerDisconnect(EventPlayerDisconnect eventPlayerDisconnect, GameEventInfo info)
    {
        var userId = eventPlayerDisconnect.Userid;

        // Userid will give you a reference to a CCSPlayerController class
        _Logger.LogInformation("Player {playerName} has disconnected!", userId.PlayerName);

        _Match?.SetPlayerDisconnected(new Player(userId));

        return HookResult.Continue;
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
        if (_Match.CurrentState < MatchState.MatchStarting && userId != null && userId.IsValid && !userId.IsBot)
        {
            // Give players max money if no match is running
            _CsServer.NextFrame(() =>
            {
                CheckMatchPlayerTeam(userId, userId.TeamNum);
            });
        }


        return HookResult.Continue;
    }

    private void OnMapStartHandler(string mapName)
    {
        if (_Match != null)
        {
            _CsServer.NextFrame(() =>
            {
                SetMatchVariable(_Match.MatchInfo.Config);
            });
        }
    }

    // TODO Add Round Events to RoundService?
    private HookResult OnRoundPreStart(EventRoundPrestart eventRoundPrestart, GameEventInfo info)
    {
        _Logger.LogInformation("OnRoundPreStart called");

        _CurrentRountState.Reset();

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

            // Update contribution score foreach player
            var players = Utilities.GetPlayers();

            foreach (var player in players)
            {
                var playerStats = _CurrentRountState.GetPlayerRoundStats(player.SteamID, player.PlayerName);

                playerStats.ContributionScore = player.Score;
            }

            var teamT = teamEntities.First(x => x.TeamNum == (int)Match.Contract.Team.Terrorist);
            var teamCT = teamEntities.First(x => x.TeamNum == (int)Match.Contract.Team.CounterTerrorist);
            var currentRound = teamT.Score + teamCT.Score;

            var isFirstHalf = currentRound <= _Match.MatchInfo.Config.MaxRounds / 2;
            _Match.SendRoundResults(new RoundResult
            {
                RoundWinner = (Match.Contract.Team)eventRoundEnd.Winner,
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

            var backupDir = Path.Combine(PugSharpDirectory, "Backup");
            if (!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }

            var configFileName = Path.Combine(backupDir, string.Create(CultureInfo.InvariantCulture, $"Match_{_Match.MatchInfo.Config.MatchId}_Round_{currentRound}.json"));

            using var configWriteStream = File.Open(configFileName, FileMode.Create);
            JsonSerializer.Serialize(configWriteStream, _Match.MatchInfo);


            // Toggle after last round in half
            if (currentRound == _Match.MatchInfo.Config.MaxRounds.Half())
            {
                _Logger.LogInformation("Switching Teams on halftime");
                _Match.SwitchTeam();
            }

            if (currentRound > _Match.MatchInfo.Config.MaxRounds)
            {
                var otRound = currentRound - _Match.MatchInfo.Config.MaxRounds;
                if (otRound == _Match.MatchInfo.Config.MaxOvertimeRounds.Half())
                {
                    _Logger.LogInformation("Switching Teams on overtime halftime");
                    _Match.SwitchTeam();
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

            // Report MVP
            if (mvp.UserId.HasValue)
            {
                var roundMvpParams = new RoundMvpParams(
                _Match.MatchInfo.Config.MatchId,
                _Match.MatchInfo.CurrentMap.MapNumber,
                _Match.MatchInfo.CurrentMap.Team1Points + _Match.MatchInfo.CurrentMap.Team2Points,
                new ApiPlayer
                {
                    UserId = mvp.UserId.Value,
                    SteamId = mvp.SteamID,
                    IsBot = mvp.IsBot,
                    Name = mvp.PlayerName,
                    Side = mvp.TeamNum,
                },
                eventRoundMvp.Reason
                );

                _ = _ApiProvider.RoundMvpAsync(roundMvpParams, CancellationToken.None);
            }
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

        if (_Match.CurrentState == MatchState.MatchRunning && eventPlayerHurt.Attacker.IsValid)
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

        if (_Match.CurrentState <= MatchState.WaitingForPlayersReady)
        {
            if (eventPlayerDeath.Userid.InGameMoneyServices != null)
            {
                int maxMoneyValue = 16000;
                //try
                //{
                //    // Use value from server if possible
                //    var maxMoneyCvar = ConVar.Find("mp_maxmoney");
                //    if (maxMoneyCvar != null)
                //    {

                //        maxMoneyValue = maxMoneyCvar.GetPrimitiveValue<int>();
                //    }
                //}
                //catch (Exception e)
                //{
                //    _Logger.LogError(e, "Error loading mp_maxmoney!");
                //}

                eventPlayerDeath.Userid.InGameMoneyServices.Account = maxMoneyValue;
            }
        }

        if (_Match.CurrentState == MatchState.MatchRunning)
        {
            var victim = eventPlayerDeath.Userid;

            var victimSide = (TeamConstants)eventPlayerDeath.Userid.TeamNum;

            var attacker = eventPlayerDeath.Attacker;

            var attackerSide = eventPlayerDeath.Attacker == null || !eventPlayerDeath.Attacker.IsValid ? TeamConstants.TEAM_INVALID : (TeamConstants)eventPlayerDeath.Attacker.TeamNum;

            CCSPlayerController? assister = eventPlayerDeath.Assister;

            var killedByBomb = eventPlayerDeath.Weapon.Equals("planted_c4", StringComparison.OrdinalIgnoreCase);
            var isSuicide = (attacker == victim) && !killedByBomb;
            var isHeadshot = eventPlayerDeath.Headshot;
            var isClutcher = false; // TODO

            var victimStats = _CurrentRountState.GetPlayerRoundStats(victim.SteamID, victim.PlayerName);

            victimStats.Dead = true;

            OnPlayerDeathHandleFirstKill(victimSide, victimStats);

            if (isSuicide)
            {
                victimStats.Suicide = true;
            }
            else if (!killedByBomb)
            {
                if (attacker != null && attacker.IsValid)
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
                                default:
                                    // Do nothing
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
                                default:
                                    // Do nothing
                                    break;
                            }

                            var hasClutched = false; // TODO

                            if (hasClutched)
                            {
                                attackerStats.Clutched = true;

                                victimStats.ClutchKills = 0;
                            }
                        }

#pragma warning disable MA0003 // Add parameter name to improve readability
                        var weaponId = Enum.Parse<CSWeaponId>(eventPlayerDeath.Weapon, true);
#pragma warning restore MA0003 // Add parameter name to improve readability

                        // Other than these constants, all knives can be found after CSWeapon_MAX_WEAPONS_NO_KNIFES.
                        // See https://sourcemod.dev/#/cstrike/enumeration.CSWeaponID
                        if (weaponId == CSWeaponId.Knife || weaponId == CSWeaponId.Knife_GG || weaponId == CSWeaponId.Knife_T ||
                            weaponId == CSWeaponId.Knife_Ghost || weaponId > CSWeaponId.Max_Weapons_No_Knifes)
                        {
                            attackerStats.KnifeKills++;
                        }
                    }
                }

                if (assister != null && assister.IsValid)
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
            else
            {
                // Do nothing
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
            const double requieredenemiesFlashedDuration = 2.5;
            if (eventPlayerBlind.BlindDuration >= requieredenemiesFlashedDuration)
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

    private HookResult OnClientCommandJoinTeam(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (_Match == null)
        {
            return HookResult.Continue;
        }

        _Logger.LogInformation("OnClientCommandJoinTeam was called!");
        if (player != null && player.IsValid)
        {
            _Logger.LogInformation("Player {playerName} tried to switch team!", player.PlayerName);
        }

        return HookResult.Stop;
    }

    #endregion

    #region Commands

    [ConsoleCommand("css_stopmatch", "Stop the current match")]
    [ConsoleCommand("ps_stopmatch", "Stop the current match")]
    [RequiresPermissions("@pugsharp/matchadmin")]
    public void OnCommandStopMatch(CCSPlayerController? player, CommandInfo command)
    {
        HandleCommand(() =>
        {
            if (_Match == null)
            {
                command.ReplyToCommand("Currently no Match is running. ");
                return;
            }

            var resetMap = _Match.MatchInfo.Config.VoteMap;
            StopMatch();
            ResetServer(resetMap);
            command.ReplyToCommand("Match stopped!");
        },
        command,
        player);
    }

    private void StopMatch()
    {
        if (_Match == null)
        {
            return;
        }

        _Match.MatchFinalized -= OnMatchFinalized;
        _Match.Dispose();
        _Match = null;
    }

    [ConsoleCommand("css_loadconfig", "Load a match config")]
    [ConsoleCommand("ps_loadconfig", "Load a match config")]
    [RequiresPermissions("@pugsharp/matchadmin")]
    public void OnCommandLoadConfig(CCSPlayerController? player, CommandInfo command)
    {
        const int requiredArgCount = 2;

        HandleCommand(() =>
        {

            if (_Match != null)
            {
                command.ReplyToCommand("Currently Match {match} is running. To stop it call ps_stopmatch");
                return;
            }

            if (command.ArgCount < requiredArgCount)
            {
                command.ReplyToCommand("Url is required as Argument!");
                return;
            }

            _Logger.LogInformation("Start loading match config!");

            var url = command.ArgByIndex(1);
            var authToken = command.ArgCount > 2 ? command.ArgByIndex(2) : string.Empty;

            command.ReplyToCommand($"Loading Config from {url}");
            var loadMatchConfigFromUrlResult = _ConfigProvider.LoadMatchConfigFromUrlAsync(url, authToken).GetAwaiter().GetResult();

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

                    var backupDir = Path.Combine(PugSharpDirectory, "Backup");
                    if (!Directory.Exists(backupDir))
                    {
                        Directory.CreateDirectory(backupDir);
                    }

                    var configFileName = Path.Combine(backupDir, $"Match_{matchConfig.MatchId}_Config.json");

                    using var configWriteStream = File.Open(configFileName, FileMode.Create);
                    {
                        JsonSerializer.Serialize(configWriteStream, matchConfig);
                    }

                    InitializeMatch(matchConfig);
                }
            );
        },
        command, player);
    }

    [ConsoleCommand("css_loadconfigfile", "Load a match config from a file")]
    [ConsoleCommand("ps_loadconfigfile", "Load a match config from a file")]
    [RequiresPermissions("@pugsharp/matchadmin")]
    public void OnCommandLoadConfigFromFile(CCSPlayerController? player, CommandInfo command)
    {
        const int requiredArgCount = 2;

        _ = HandleCommandAsync(async () =>
        {

            if (_Match != null)
            {
                command.ReplyToCommand("Currently Match {match} is running. To stop it call ps_stopmatch");
                return;
            }

            if (command.ArgCount != requiredArgCount)
            {
                command.ReplyToCommand("FileName is required as Argument! Path have to be put in \"pathToConfig\"");
                return;
            }

            _Logger.LogInformation("Start loading match config!");
            var fileName = command.ArgByIndex(1);

            command.ReplyToCommand($"Loading Config from file {fileName}");
            var loadMatchConfigFromFileResult = await _ConfigProvider.LoadMatchConfigFromFileAsync(fileName).ConfigureAwait(false);

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
        command,
        player);
    }

    [ConsoleCommand("css_restorematch", "Restore a match")]
    [ConsoleCommand("ps_restorematch", "Restore a match")]
    [RequiresPermissions("@pugsharp/matchadmin")]
    public async void OnCommandRestoreMatch(CCSPlayerController? player, CommandInfo command)
    {
        const int requiredArgCount = 2;
        const int argMatchIdIndex = 1;
        const int argRoundNumberIndex = 2;

        await HandleCommandAsync(async () =>
        {
            if (_Match != null)
            {
                command.ReplyToCommand("Currently Match {match} is running. To stop it call ps_stopmatch");
                return;
            }

            if (command.ArgCount < requiredArgCount)
            {
                command.ReplyToCommand("MatchId is required as Argument!");
                return;
            }

            var matchId = int.Parse(command.ArgByIndex(argMatchIdIndex), CultureInfo.InvariantCulture);

            // Backups are stored in csgo directory
            var csgoDirectory = Path.GetDirectoryName(PugSharpDirectory);
            if (csgoDirectory == null)
            {
                command.ReplyToCommand("Csgo directory not found!");
                return;
            }

            int roundToRestore;
            if (command.ArgCount == argRoundNumberIndex)
            {
                var fileNameWildcard = string.Create(CultureInfo.InvariantCulture, $"PugSharp_Match_{matchId}_round*");

                var files = Directory.EnumerateFiles(csgoDirectory, fileNameWildcard);
                foreach (var file in files)
                {
                    _Logger.LogInformation("found posisble Backup: {file} ", file);
                }

                const int last2Chars = 2;
                roundToRestore = files.Select(x => Path.GetFileNameWithoutExtension(x)).Select(x => x[^last2Chars..]).Select(x => int.Parse(x, CultureInfo.InvariantCulture)).Max();
            }
            else
            {
                if (!int.TryParse(command.ArgByIndex(argRoundNumberIndex), CultureInfo.InvariantCulture, out roundToRestore))
                {
                    command.ReplyToCommand("second argument for round index has to be numeric");
                    return;
                }
            }

            _Logger.LogInformation("Start restoring match {matchid}!", matchId);


            var roundBackupFile = string.Create(CultureInfo.InvariantCulture, $"PugSharp_Match_{matchId}_round{roundToRestore:D2}.txt");

            if (!File.Exists(Path.Combine(_CsServer.GameDirectory, "csgo", roundBackupFile)))
            {
                command.ReplyToCommand($"RoundBackupFile {roundBackupFile} not found");
                return;
            }

            var matchInfoFileName = Path.Combine(PugSharpDirectory, "Backup", string.Create(CultureInfo.InvariantCulture, $"Match_{matchId}_Round_{roundToRestore}.json"));
            if (!File.Exists(matchInfoFileName))
            {
                command.ReplyToCommand($"MatchInfoFile {matchInfoFileName} not found");
                return;
            }

            var matchInfoStream = File.OpenRead(matchInfoFileName);
            await using (matchInfoStream.ConfigureAwait(false))
            {
                var matchInfo = await JsonSerializer.DeserializeAsync<MatchInfo>(matchInfoStream).ConfigureAwait(false);

                if (matchInfo == null)
                {
                    command.ReplyToCommand($"MatchInfoFile {matchInfoFileName} could not be loaded!");
                    return;
                }

                InitializeMatch(matchInfo, roundBackupFile);
            }
        },
        command,
        player).ConfigureAwait(false);
    }

    [ConsoleCommand("css_creatematch", "Create a match without predefined config")]
    [ConsoleCommand("ps_creatematch", "Create a match without predefined config")]
    [RequiresPermissions("@pugsharp/matchadmin")]
    public void OnCommandCreateMatch(CCSPlayerController? player, CommandInfo command)
    {
        HandleCommand(() =>
        {
            if (_Match != null)
            {
                command.ReplyToCommand("Currently Match {match} is running. To stop it call ps_stopmatch");
                return;
            }

            _ConfigCreator = new ConfigCreator();
            _ConfigCreator.Config.Maplist.Add(_CsServer.CurrentMap);

            command.ReplyToCommand("Creating a match started!");
            command.ReplyToCommand("!addmap <mapname> to add a map!");
            command.ReplyToCommand("!removemap <mapname> to remove a map!");
            command.ReplyToCommand("!startmatch <mapname> to start the match!");
            command.ReplyToCommand("!maxrounds <rounds> to set max match rounds!");
            command.ReplyToCommand("!maxovertimerounds <rounds> to set max overtime rounds!");
            command.ReplyToCommand("!playersperteam <players> to set players per team!");
            command.ReplyToCommand("!matchinfo to show current match configuration!");
        },
        command,
        player);
    }

    [ConsoleCommand("css_startmatch", "Create a match without predefined config")]
    [ConsoleCommand("ps_startmatch", "Create a match without predefined config")]
    [RequiresPermissions("@pugsharp/matchadmin")]
    public void OnCommandStartMatch(CCSPlayerController? player, CommandInfo command)
    {
        HandleCommand(() =>
        {
            if (_Match != null)
            {
                command.ReplyToCommand("Currently Match {match} is running. To stop it call ps_stopmatch");
                return;
            }

            var matchConfig = _ConfigCreator.Config;
            if (matchConfig.Maplist.Count == 0)
            {
                command.ReplyToCommand("Can not start match. At least one map is required!");
                return;
            }

            InitializeMatch(matchConfig);
        },
        command,
        player);
    }

    [ConsoleCommand("css_addmap", "Adds a map to the map pool")]
    [ConsoleCommand("ps_addmap", "Adds a map to the map pool")]
    [RequiresPermissions("@pugsharp/matchadmin")]
    public void OnCommandAddMap(CCSPlayerController? player, CommandInfo command)
    {
        const int requiredArgCount = 2;
        HandleCommand(() =>
        {
            if (!IsConfigCreatorAvailable(command))
            {
                return;
            }

            if (command.ArgCount != requiredArgCount)
            {
                command.ReplyToCommand("A map name is required to be added!");
                return;
            }

            var mapName = command.ArgByIndex(1);
            if (!_ConfigCreator.Config.Maplist.Contains(mapName, StringComparer.OrdinalIgnoreCase))
            {
                _ConfigCreator.Config.Maplist.Add(mapName);
            }

            command.ReplyToCommand($"Added {mapName}. Current maps are {string.Join(", ", _ConfigCreator.Config.Maplist)}");
        },
        command,
        player);
    }

    [ConsoleCommand("css_removemap", "Removes a map to the map pool")]
    [ConsoleCommand("ps_removemap", "Removes a map to the map pool")]
    [RequiresPermissions("@pugsharp/matchadmin")]
    public void OnCommandRemoveMap(CCSPlayerController? player, CommandInfo command)
    {
        const int requiredArgCount = 2;
        HandleCommand(() =>
        {
            if (!IsConfigCreatorAvailable(command))
            {
                return;
            }

            if (command.ArgCount != requiredArgCount)
            {
                command.ReplyToCommand("A map name is required to be removed!");
                return;
            }

            var mapName = command.ArgByIndex(1);
            if (_ConfigCreator.Config.Maplist.Remove(mapName))
            {
                command.ReplyToCommand($"Removed {mapName}. Current maps are {string.Join(", ", _ConfigCreator.Config.Maplist)}");
            }
            else
            {
                command.ReplyToCommand($"Could not remove {mapName}. Current maps are {string.Join(", ", _ConfigCreator.Config.Maplist)}");
            }
        },
        command,
        player);
    }

    [ConsoleCommand("css_maxrounds", "Sets max rounds for the match")]
    [ConsoleCommand("ps_maxrounds", "Sets max rounds for the match")]
    [RequiresPermissions("@pugsharp/matchadmin")]
    public void OnCommandMaxRounds(CCSPlayerController? player, CommandInfo command)
    {
        const int requiredArgCount = 2;
        HandleCommand(() =>
        {
            if (!IsConfigCreatorAvailable(command))
            {
                return;
            }

            if (command.ArgCount != requiredArgCount)
            {
                command.ReplyToCommand("A number of rounds is required!");
                return;
            }

            if (!int.TryParse(command.ArgByIndex(1), CultureInfo.InvariantCulture, out var maxRounds))
            {
                command.ReplyToCommand("Max rounds have to be an number!");
                return;
            }

            if (maxRounds <= 0)
            {
                command.ReplyToCommand("Max rounds have to be greater than 0!");
                return;
            }

            var oldMaxRounds = _ConfigCreator.Config.MaxRounds;
            _ConfigCreator.Config.MaxRounds = maxRounds;
            command.ReplyToCommand($"Changed max rounds from {oldMaxRounds} to {maxRounds}");
        },
        command,
        player);
    }

    [ConsoleCommand("css_maxovertimerounds", "Sets max overtime rounds for the match")]
    [ConsoleCommand("ps_maxovertimerounds", "Sets max overtime rounds for the match")]
    [RequiresPermissions("@pugsharp/matchadmin")]
    public void OnCommandMaxOvertimeRounds(CCSPlayerController? player, CommandInfo command)
    {
        const int requiredArgCount = 2;
        HandleCommand(() =>
        {
            if (!IsConfigCreatorAvailable(command))
            {
                return;
            }

            if (command.ArgCount != requiredArgCount)
            {
                command.ReplyToCommand("A number of rounds is required!");
                return;
            }

            if (!int.TryParse(command.ArgByIndex(1), CultureInfo.InvariantCulture, out var maxOvertimeRounds))
            {
                command.ReplyToCommand("Max overtime rounds have to be an number!");
                return;
            }

            if (maxOvertimeRounds <= 0)
            {
                command.ReplyToCommand("Max overtime rounds have to be greater than 0!");
                return;
            }

            var oldMaxRounds = _ConfigCreator.Config.MaxOvertimeRounds;
            _ConfigCreator.Config.MaxOvertimeRounds = maxOvertimeRounds;
            command.ReplyToCommand($"Changed max overtime rounds from {oldMaxRounds} to {maxOvertimeRounds}");
        },
        command,
        player);
    }

    [ConsoleCommand("css_playersperteam", "Sets number of players per team for the match")]
    [ConsoleCommand("ps_playersperteam", "Sets number of players per team for the match")]
    [RequiresPermissions("@pugsharp/matchadmin")]
    public void OnCommandPlayersPerTeam(CCSPlayerController? player, CommandInfo command)
    {
        const int requiredArgCount = 2;
        HandleCommand(() =>
        {
            if (!IsConfigCreatorAvailable(command))
            {
                return;
            }

            if (command.ArgCount != requiredArgCount)
            {
                command.ReplyToCommand("Players per team is required!");
                return;
            }

            if (!int.TryParse(command.ArgByIndex(1), CultureInfo.InvariantCulture, out var playersPerTeam))
            {
                command.ReplyToCommand("Players per team have to be an number!");
                return;
            }

            if (playersPerTeam <= 0)
            {
                command.ReplyToCommand("Players per team have to be greater than 0!");
                return;
            }

            var oldPlayersPerTeam = _ConfigCreator.Config.PlayersPerTeam;
            _ConfigCreator.Config.PlayersPerTeam = playersPerTeam;
            _ConfigCreator.Config.MinPlayersToReady = playersPerTeam;
            command.ReplyToCommand($"Changed players per team from {oldPlayersPerTeam} to {playersPerTeam}");
        },
        command,
        player);
    }

    private bool IsConfigCreatorAvailable(CommandInfo command)
    {
        if (_Match != null)
        {
            command.ReplyToCommand("Currently Match {match} is running. To stop it call ps_stopmatch");
            return false;
        }

        if (_ConfigCreator == null)
        {
            command.ReplyToCommand("To Configure a new match you have to call ps_creatematch first");
            return false;
        }

        return true;
    }

    [ConsoleCommand("css_matchinfo", "Serialize match to JSON on console")]
    [ConsoleCommand("ps_matchinfo", "Serialize match to JSON on console")]
    [RequiresPermissions("@pugsharp/matchadmin")]
    public void OnCommandMatchInfo(CCSPlayerController? player, CommandInfo command)
    {
        HandleCommand(() =>
        {
            if (_Match == null && _ConfigCreator == null)
            {
                command.ReplyToCommand("Currently no match is running. Matchinfo is unavailable!");
                return;
            }

            var config = _Match?.MatchInfo?.Config ?? _ConfigCreator.Config;

            command.ReplyToCommand($"Info Match {config.MatchId}");
            command.ReplyToCommand($"Maplist: {string.Join(", ", config.Maplist)}");
            command.ReplyToCommand($"Number of Maps: {config.NumMaps}");
            command.ReplyToCommand($"Players per Team: {config.PlayersPerTeam}");
            command.ReplyToCommand($"Max rounds: {config.MaxRounds}");
            command.ReplyToCommand($"Max overtime rounds: {config.MaxOvertimeRounds}");
            command.ReplyToCommand($"Vote timeout: {config.VoteTimeout}");
            command.ReplyToCommand($"Allow suicide: {config.AllowSuicide}");
            command.ReplyToCommand($"Team mode: {config.TeamMode}");
        },
        command,
        player);
    }

    [ConsoleCommand("css_dumpmatch", "Serialize match to JSON on console")]
    [ConsoleCommand("ps_dumpmatch", "Serialize match to JSON on console")]
    [RequiresPermissions("@pugsharp/matchadmin")]
    public void OnCommandDumpMatch(CCSPlayerController? player, CommandInfo command)
    {
        HandleCommand(() =>
        {
            _Logger.LogInformation("################ dump match ################");
            _Logger.LogInformation("{matchJson}", JsonSerializer.Serialize(_Match));
            _Logger.LogInformation("################ dump match ################");
        },
        command,
        player);
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

           _Match.TogglePlayerIsReady(matchPlayer);
       },
       command,
       player);
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
        command,
        player);
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
        command,
        player);
    }

    [ConsoleCommand("css_kill", "Kills the calling player")]
    [ConsoleCommand("ps_kill", "Kills the calling player")]
    [ConsoleCommand("css_suicide", "Kills the calling player")]
    [ConsoleCommand("ps_suicide", "Kills the calling player")]
    public void OnCommandKillCalled(CCSPlayerController? player, CommandInfo command)
    {
        HandleCommand(() =>
        {
            if (player == null || !player.IsValid)
            {
                command.ReplyToCommand("Command is only possible for valid players!");
                return;
            }

            if (_Match?.MatchInfo.Config?.AllowSuicide != true)
            {
                command.ReplyToCommand("Suicide is not allowed during this match!");
            }

#pragma warning disable MA0003 // Add parameter name to improve readability
            player.Pawn.Value.CommitSuicide(true, true);
#pragma warning restore MA0003 // Add parameter name to improve readability
        },
        command,
        player);
    }


    private void HandleCommand(Action commandAction, CommandInfo command, CCSPlayerController? player = null, string? args = null, [CallerMemberName] string? commandMethod = null)
    {
        var commandName = commandMethod?.Replace("OnCommand", "", StringComparison.OrdinalIgnoreCase) ?? commandAction.Method.Name;
        try
        {
            if (player != null)
            {
                _Logger.LogInformation("Command \"{commandName} {args}\" called by player with SteamID {steamId}.", commandName, args ?? string.Empty, player.SteamID);
            }
            else
            {
                _Logger.LogInformation("Command \"{commandName} {args}\" called.", commandName, args ?? string.Empty);
            }

            commandAction();
        }
        catch (Exception e)
        {
            _Logger.LogError(e, "Error executing command {command}", commandName);
            command.ReplyToCommand($"Error executing command \"{commandName}\"!");
        }
    }

    private async Task HandleCommandAsync(Func<Task> commandAction, CommandInfo command, CCSPlayerController? player = null, string? args = null, [CallerMemberName] string? commandMethod = null)
    {
        var commandName = commandMethod?.Replace("OnCommand", "", StringComparison.OrdinalIgnoreCase) ?? commandAction.Method.Name;
        try
        {
            if (player != null)
            {
                _Logger.LogInformation("Command \"{commandName} {args}\" called by player with SteamID {steamId}.", commandName, args ?? string.Empty, player.SteamID);
            }
            else
            {
                _Logger.LogInformation("Command \"{commandName} {args}\" called.", commandName, args ?? string.Empty);
            }

            await commandAction().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _Logger.LogError(e, "Error executing command {command}", commandName);
        }
    }


    #endregion

    private async Task ConfigLoaderTask()
    {
        while (await _ConfigTimer.WaitForNextTickAsync(_CancellationTokenSource.Token).ConfigureAwait(false))
        {
            if ((_Match == null || _Match.CurrentState == MatchState.WaitingForPlayersConnectedReady || _Match.CurrentState == MatchState.WaitingForPlayersReady) && Utilities.GetPlayers().Count(x => !x.IsBot) == 0)
            {
                // TODO Besseren Platz suchen!
                _CsServer.LoadAndExecuteConfig("warmup.cfg");
            }
        }
    }

    #region Utils

    // TODO put somewhere else?
    private IReadOnlyDictionary<ulong, IPlayerRoundResults> CreatePlayerResults()
    {
        var dict = new Dictionary<ulong, IPlayerRoundResults>();

        foreach (var kvp in _CurrentRountState.PlayerStats)
        {
            dict[kvp.Key] = kvp.Value;
        }

        return dict;
    }


    private void OnPlayerDeathHandleFirstKill(TeamConstants victimSide, PlayerRoundStats victimStats)
    {
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
                default:
                    // Do nothing
                    break;
            }
        }
    }

    private void SetMatchVariable(MatchConfig matchConfig)
    {
        _Logger.LogInformation("Start set match variables");

        _CsServer.UpdateConvar("mp_overtime_maxrounds", matchConfig.MaxOvertimeRounds);
        _CsServer.UpdateConvar("mp_maxrounds", matchConfig.MaxRounds);

        // Set T Name, can be changed to ConVar if issue https://github.com/roflmuffin/CounterStrikeSharp/issues/45 is fixed
        _CsServer.ExecuteCommand($"mp_teamname_1 {matchConfig.Team2.Name}");
        _CsServer.ExecuteCommand($"mp_teamflag_1 {matchConfig.Team2.Flag}");

        // Set CT Name, can be changed to ConVar if issue https://github.com/roflmuffin/CounterStrikeSharp/issues/45 is fixed
        _CsServer.ExecuteCommand($"mp_teamname_2 {matchConfig.Team1.Name}");
        _CsServer.ExecuteCommand($"mp_teamflag_2 {matchConfig.Team1.Flag}");

        _Logger.LogInformation("Set match variables done");
    }

    private void InitializeMatch(MatchConfig matchConfig)
    {
        ResetForMatch(matchConfig);
        var matchFactory = _ServiceProvider.GetRequiredService<MatchFactory>();
        _Match = matchFactory.CreateMatch(matchConfig);
        _Match.MatchFinalized += OnMatchFinalized;
        KickNonMatchPlayers();
    }

    private void InitializeMatch(MatchInfo matchInfo, string roundBackupFile)
    {
        ResetForMatch(matchInfo.Config);
        var matchFactory = _ServiceProvider.GetRequiredService<MatchFactory>();
        _Match = matchFactory.CreateMatch(matchInfo, roundBackupFile);
        _Match.MatchFinalized += OnMatchFinalized;
        KickNonMatchPlayers();
    }

    private void OnMatchFinalized(object? sender, MatchFinalizedEventArgs e)
    {
        StopMatch();
    }

    private void ResetForMatch(MatchConfig matchConfig)
    {
        ResetServer(matchConfig.VoteMap);

        _ApiProvider.ClearApiProviders();

        if (!string.IsNullOrEmpty(matchConfig.EventulaApistatsUrl))
        {
            var apiStats = _ServiceProvider.GetRequiredService<ApiStats.ApiStats>();
            apiStats.Initialize(matchConfig.EventulaApistatsUrl, matchConfig.EventulaApistatsToken ?? string.Empty);
            _ApiProvider.AddApiProvider(apiStats);
        }

        if (!string.IsNullOrEmpty(matchConfig.G5ApiUrl))
        {
            var g5Stats = _ServiceProvider.GetRequiredService<Api.G5Api.G5ApiClient>();
            g5Stats.Initialize(matchConfig.G5ApiUrl, matchConfig.G5ApiHeader ?? string.Empty, matchConfig.G5ApiHeaderValue ?? string.Empty);
            var g5ApiProvider = _ServiceProvider.GetRequiredService<G5ApiProvider>();
            _Plugin.RegisterConsoleCommandAttributeHandlers(g5ApiProvider);
            _ApiProvider.AddApiProvider(g5ApiProvider);
        }

        var jsonProvider = _ServiceProvider.GetRequiredService<JsonApiProvider>();
        jsonProvider.Initialize(Path.Combine(PugSharpDirectory, "Stats"));
        _ApiProvider.AddApiProvider(jsonProvider);

        SetMatchVariable(matchConfig);

        _Match?.Dispose();
    }

    private void KickNonMatchPlayers()
    {
        if (_Match == null)
        {
            return;
        }

        var players = Utilities.GetPlayers().Where(x => x.IsValid && !x.IsHLTV);
        foreach (var player in players)
        {
            if (!_Match.TryAddPlayer(new Player(player)))
            {
                player.Kick();
            }
        }
    }

    private void ResetServer(string map)
    {
        _CsServer.StopDemoRecording();
        _CsServer.SwitchMap(map);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_DisposedValue)
        {
            if (disposing)
            {
                StopMatch();
            }

            _DisposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }


    #endregion
}
