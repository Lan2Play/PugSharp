﻿using System.Globalization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PugSharp.Api.Contract;
using PugSharp.ApiStats;
using PugSharp.Match.Contract;
using PugSharp.Server.Contract;
using PugSharp.Shared;
using PugSharp.Translation;
using PugSharp.Translation.Properties;

using Stateless;
using Stateless.Graph;

namespace PugSharp.Match;

public class Match : IDisposable
{
    private const int _Kill1 = 1;
    private const int _Kill2 = 2;
    private const int _Kill3 = 3;
    private const int _Kill4 = 4;
    private const int _Kill5 = 5;
    private const int _NumOfMatchLiveMessages = 10;
    private const int _TimeBetweenDelayMessages = 10;
    private readonly IServiceProvider _ServiceProvider;
    private readonly ILogger<Match> _Logger;

    private readonly System.Timers.Timer _VoteTimer = new();
    private readonly System.Timers.Timer _ReadyReminderTimer = new(10000);
    private readonly IApiProvider _ApiProvider;
    private readonly ITextHelper _TextHelper;
    private readonly ICsServer _CsServer;
    private readonly string _RoundBackupFile = string.Empty;
    private readonly StateMachine<MatchState, MatchCommand> _MatchStateMachine;

    private DemoUploader? _DemoUploader;
    private readonly List<Vote> _TeamVotes = new() { new("T"), new("CT") };
    private readonly ICssDispatcher _Dispatcher;
    private List<Vote> _MapsToSelect = new List<Vote>();
    private MatchTeam? _CurrentMatchTeamToVote;
    private bool _DisposedValue;

    public MatchState CurrentState => _MatchStateMachine.State;

    public MatchInfo MatchInfo { get; private set; }

    public event EventHandler<MatchFinalizedEventArgs>? MatchFinalized;

    public IEnumerable<MatchPlayer> AllMatchPlayers => MatchInfo?.MatchTeam1.Players.Concat(MatchInfo.MatchTeam2.Players) ?? [];

    internal Match(IServiceProvider serviceProvider, ILogger<Match> logger, IApiProvider apiProvider, ITextHelper textHelper, ICsServer csServer, ICssDispatcher cssDispatcher, Config.MatchConfig matchConfig) :
        this(serviceProvider, logger, apiProvider, textHelper, csServer, cssDispatcher)
    {
        Initialize(new MatchInfo(matchConfig));
        InitializeStateMachine();
    }

    internal Match(IServiceProvider serviceProvider, ILogger<Match> logger, IApiProvider apiProvider, ITextHelper textHelper, ICsServer csServer, ICssDispatcher cssDispatcher, MatchInfo matchInfo, string roundBackupFile) :
        this(serviceProvider, logger, apiProvider, textHelper, csServer, cssDispatcher)
    {
        _RoundBackupFile = roundBackupFile;
        Initialize(matchInfo);
        InitializeStateMachine();
        _Logger.LogInformation("Continue Match on map {MapNumber}({MapName})!", MatchInfo!.CurrentMap.MapNumber, MatchInfo.CurrentMap.MapName);
    }

    private Match(IServiceProvider serviceProvider, ILogger<Match> logger, IApiProvider apiProvider, ITextHelper textHelper, ICsServer csServer, ICssDispatcher cssDispatcher)
    {
        _ServiceProvider = serviceProvider;
        _Logger = logger;
        _ApiProvider = apiProvider;
        _TextHelper = textHelper;
        _CsServer = csServer;
        _Dispatcher = cssDispatcher;
        _MatchStateMachine = new StateMachine<MatchState, MatchCommand>(MatchState.None);

        MatchInfo ??= default!;
    }

    private void Initialize(MatchInfo matchInfo)
    {
        if (MatchInfo != null)
        {
            throw new NotSupportedException("Initialize can only be called once!");
        }

        if (matchInfo.Config.Maplist.Count < matchInfo.Config.NumMaps)
        {
            throw new NotSupportedException(string.Create(CultureInfo.InvariantCulture, $"Can not create Match without the required number of maps! At lease {matchInfo.Config.NumMaps} are required!"));
        }

        MatchInfo = matchInfo;
        _VoteTimer.Interval = MatchInfo.Config.VoteTimeout;
        _VoteTimer.Elapsed += VoteTimer_Elapsed;
        _ReadyReminderTimer.Elapsed += ReadyReminderTimer_Elapsed;

        MatchInfo.CurrentMap = matchInfo.MatchMaps.LastOrDefault(x => !string.IsNullOrEmpty(x.MapName)) ?? matchInfo.MatchMaps[matchInfo.MatchMaps.Count - 1];
        _Logger.LogInformation("Continue Match on map {MapNumber}({MapName})!", MatchInfo.CurrentMap.MapNumber, MatchInfo.CurrentMap.MapName);

        if (!string.IsNullOrEmpty(MatchInfo.Config.EventulaDemoUploadUrl) && !string.IsNullOrEmpty(MatchInfo.Config.EventulaApistatsToken))
        {
            _DemoUploader = _ServiceProvider.GetRequiredService<DemoUploader>();
            _DemoUploader.Initialize(MatchInfo.Config.EventulaDemoUploadUrl, MatchInfo.Config.EventulaApistatsToken);
        }
    }

#pragma warning disable MA0051 // Method is too long
    private void InitializeStateMachine()
#pragma warning restore MA0051 // Method is too long
    {
        _MatchStateMachine.Configure(MatchState.None)
            .PermitDynamicIf(MatchCommand.LoadMatch, () => HasRestoredMatch() ? MatchState.RestoreMatch : MatchState.WaitingForPlayersConnectedReady);

        _MatchStateMachine.Configure(MatchState.WaitingForPlayersConnectedReady)
            .PermitDynamicIf(MatchCommand.PlayerReady, () => HasRestoredMatch() ? MatchState.MatchRunning : MatchState.DefineTeams, AllPlayersAreReady)
            .OnEntry(StartWarmup)
            .OnEntry(SetAllPlayersNotReady)
            .OnEntry(StartReadyReminder)
            .OnExit(StopReadyReminder);

        _MatchStateMachine.Configure(MatchState.DefineTeams)
            .Permit(MatchCommand.TeamsDefined, MatchState.MapVote)
            .OnEntry(ContinueIfDefault)
            .OnEntry(ContinueIfPlayerSelect)
            .OnEntry(ScrambleTeams);

        _MatchStateMachine.Configure(MatchState.MapVote)
            .PermitReentryIf(MatchCommand.VoteMap, MapIsNotSelected)
            .PermitIf(MatchCommand.VoteMap, MatchState.TeamVote, MapIsSelected)
            .OnEntry(InitializeMapsToVote)
            .OnEntry(SendRemainingMapsToVotingTeam)
            .OnExit(RemoveBannedMap);

        _MatchStateMachine.Configure(MatchState.TeamVote)
            .Permit(MatchCommand.VoteTeam, MatchState.SwitchMap)
            .OnEntry(SendTeamVoteToVotingTeam)
            .OnExit(SetSelectedTeamSide);

        _MatchStateMachine.Configure(MatchState.SwitchMap)
            .Permit(MatchCommand.SwitchMap, MatchState.WaitingForPlayersReady)
            .OnEntry(SwitchToMatchMap);

        _MatchStateMachine.Configure(MatchState.WaitingForPlayersReady)
            .PermitIf(MatchCommand.PlayerReady, MatchState.MatchStarting, AllPlayersAreReady)
            .OnEntry(SetAllPlayersNotReady)
            .OnEntry(StartReadyReminder)
            .OnExit(StopReadyReminder);

        _MatchStateMachine.Configure(MatchState.MatchStarting)
            .Permit(MatchCommand.StartMatch, MatchState.MatchRunning)
            .OnEntry(StartMatch);

        _MatchStateMachine.Configure(MatchState.MatchRunning)
            .Permit(MatchCommand.DisconnectPlayer, MatchState.MatchPaused)
            .Permit(MatchCommand.Pause, MatchState.MatchPaused)
            .PermitIf(MatchCommand.CompleteMap, MatchState.MapCompleted)
            .OnEntry(MatchLive);

        _MatchStateMachine.Configure(MatchState.MatchPaused)
            .PermitIf(MatchCommand.ConnectPlayer, MatchState.MatchRunning, AllPlayersAreConnected)
            .PermitIf(MatchCommand.Unpause, MatchState.MatchRunning, AllTeamsUnpaused)
            .OnEntry(PauseMatch)
            .OnExit(UnpauseMatch);

        _MatchStateMachine.Configure(MatchState.RestoreMatch)
            .PermitIf(MatchCommand.ConnectPlayer, MatchState.MatchRunning, AllPlayersAreConnected)
            .OnEntry(PauseMatch)
            .OnExit(() =>
            {
                if (HasRestoredMatch())
                {
                    _CsServer.RestoreBackup(_RoundBackupFile);
                }
            })
            .OnExit(UnpauseMatch);

        _MatchStateMachine.Configure(MatchState.MapCompleted)
            .PermitIf(MatchCommand.CompleteMatch, MatchState.MatchCompleted, AllMapsArePlayed)
            .PermitIf(MatchCommand.CompleteMatch, MatchState.WaitingForPlayersConnectedReady, NotAllMapsArePlayed)
            .OnEntry(SendMapResults)
            .OnEntry(TryCompleteMatch);


        _MatchStateMachine.Configure(MatchState.MatchCompleted)
            .OnEntryAsync(CompleteMatchAsync);

        _MatchStateMachine.OnTransitioned(OnMatchStateChanged);

        _MatchStateMachine.Fire(MatchCommand.LoadMatch);
    }

    private void ScrambleTeams()
    {
        if (MatchInfo.Config.TeamMode == Config.TeamMode.Scramble)
        {
            var randomizedPlayers = AllMatchPlayers.Randomize().ToList();

            MatchInfo.MatchTeam1.Players.Clear();
            MatchInfo.MatchTeam1.Players.AddRange(randomizedPlayers.Take(randomizedPlayers.Count.Half()));

            MatchInfo.MatchTeam2.Players.Clear();
            MatchInfo.MatchTeam2.Players.AddRange(randomizedPlayers.Skip(randomizedPlayers.Count.Half()));

            TryFireState(MatchCommand.TeamsDefined);
        }
    }

    private void ContinueIfDefault()
    {
        if (MatchInfo.Config.TeamMode == Config.TeamMode.Default)
        {
            TryFireState(MatchCommand.TeamsDefined);
        }
    }

    private void ContinueIfPlayerSelect()
    {
        if (MatchInfo.Config.TeamMode == Config.TeamMode.PlayerSelect)
        {
            TryFireState(MatchCommand.TeamsDefined);
        }
    }

    private void StartWarmup()
    {
        _CsServer.LoadAndExecuteConfig("warmup.cfg");
    }

    private bool HasRestoredMatch()
    {
        return !string.IsNullOrEmpty(MatchInfo.CurrentMap.MapName);
    }

    private void StartReadyReminder()
    {
        _Logger.LogInformation("Start ReadyReminder");
        _ReadyReminderTimer.Start();
    }

    private void StopReadyReminder()
    {
        _Logger.LogInformation("Stop ReadyReminder");
        _ReadyReminderTimer.Stop();
        foreach (var player in AllMatchPlayers)
        {
            player.Player.Clan = string.Empty;
            player.IsReady = true;
        }
    }

    private void UnpauseMatch()
    {
        _CsServer.UnpauseMatch();
    }

    private void PauseMatch()
    {
        if (MatchInfo == null)
        {
            _Logger.LogError("Can not pause match without a matchInfo!");
            return;
        }

        MatchInfo.MatchTeam1.IsPaused = true;
        MatchInfo.MatchTeam2.IsPaused = true;

        _CsServer.PauseMatch();
    }

    private void StartMatch()
    {
        if (MatchInfo == null)
        {
            _Logger.LogError("Can not start match without a matchInfo!");
            return;
        }

        // Disable cheats
        _CsServer.DisableCheats();

        // End warmup
        _CsServer.EndWarmup();

        // Load live config
        _CsServer.LoadAndExecuteConfig("live.cfg");

        // Restart Game to reset everything
        _CsServer.RestartGame();

        string prefix = $"PugSharp_Match_{MatchInfo.Config.MatchId}";
        _CsServer.SetupRoundBackup(prefix);

        var formattedDateTime = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        var demoFileName = $"PugSharp_Match_{MatchInfo.Config.MatchId}_{formattedDateTime}";
        string demoDirectory = Path.Combine(_CsServer.GameDirectory, "csgo", "PugSharp", "Demo");
        MatchInfo.DemoFile = _CsServer.StartDemoRecording(demoDirectory, demoFileName);

        _CsServer.PrintToChatAll(_TextHelper.GetText(nameof(Resources.PugSharp_Match_Info_StartMatch), MatchInfo.MatchTeam1.TeamConfig.Name, MatchInfo.MatchTeam1.CurrentTeamSide, MatchInfo.MatchTeam2.TeamConfig.Name, MatchInfo.MatchTeam2.CurrentTeamSide));

        _ = _ApiProvider.GoingLiveAsync(new GoingLiveParams(MatchInfo.Config.MatchId, MatchInfo.CurrentMap.MapName, MatchInfo.CurrentMap.MapNumber), CancellationToken.None);

        TryFireState(MatchCommand.StartMatch);
    }

    private void SendMapResults()
    {
        if (MatchInfo.CurrentMap.Winner == null)
        {
            throw new NotSupportedException("Map Winner is not yet set. Can not send map results");
        }

        _ = _ApiProvider.FinalizeMapAsync(new MapResultParams(MatchInfo.Config.MatchId, MatchInfo.CurrentMap.Winner.TeamConfig.Name, MatchInfo.CurrentMap.Team1Points, MatchInfo.CurrentMap.Team2Points, MatchInfo.CurrentMap.MapNumber), CancellationToken.None);
    }

    public void SendRoundResults(IRoundResults roundResults)
    {
        var teamInfo1 = new TeamInfo
        {
            TeamId = MatchInfo.Config.Team1.Id,
            TeamName = MatchInfo.Config.Team1.Name,
        };

        var teamInfo2 = new TeamInfo
        {
            TeamId = MatchInfo.Config.Team2.Id,
            TeamName = MatchInfo.Config.Team2.Name,
        };

        UpdateStats(roundResults.PlayerResults);

        var team1Results = MatchInfo.MatchTeam1.CurrentTeamSide == Team.Terrorist ? roundResults.TRoundResult : roundResults.CTRoundResult;
        var team2Results = MatchInfo.MatchTeam2.CurrentTeamSide == Team.Terrorist ? roundResults.TRoundResult : roundResults.CTRoundResult;

        MatchInfo.CurrentMap.Team1Points = team1Results.Score;
        MatchInfo.CurrentMap.Team2Points = team2Results.Score;

        _Logger.LogInformation("Team 1: {TeamSide} : {TeamScore}", MatchInfo.MatchTeam1.CurrentTeamSide, team1Results.Score);
        _Logger.LogInformation("Team 2: {TeamSide} : {TeamScore}", MatchInfo.MatchTeam2.CurrentTeamSide, team2Results.Score);

        var mapTeamInfo1 = new MapTeamInfo
        {
            StartingSide = MatchInfo.MatchTeam1.StartingTeamSide == Team.Terrorist ? TeamSide.T : TeamSide.CT,
            Score = team1Results.Score,
            ScoreT = team1Results.ScoreT,
            ScoreCT = team1Results.ScoreCT,
            Players = MatchInfo.CurrentMap.PlayerMatchStatistics
                            .Where(a => MatchInfo.MatchTeam1.Players.Select(player => player.Player.SteamID).Contains(a.Key))
                            .ToDictionary(p => p.Key.ToString(CultureInfo.InvariantCulture), p => CreatePlayerStatistics(p.Value), StringComparer.OrdinalIgnoreCase),
        };

        var mapTeamInfo2 = new MapTeamInfo
        {
            StartingSide = MatchInfo.MatchTeam2.StartingTeamSide == Team.Terrorist ? TeamSide.T : TeamSide.CT,
            Score = team2Results.Score,
            ScoreT = team2Results.ScoreT,
            ScoreCT = team2Results.ScoreCT,

            Players = MatchInfo.CurrentMap.PlayerMatchStatistics
                            .Where(a => MatchInfo.MatchTeam2.Players.Select(player => player.Player.SteamID).Contains(a.Key))
                            .ToDictionary(p => p.Key.ToString(CultureInfo.InvariantCulture), p => CreatePlayerStatistics(p.Value), StringComparer.OrdinalIgnoreCase),
        };

        var winnerTeam = GetMatchTeam(roundResults.RoundWinner);
        if (winnerTeam == null)
        {
            _Logger.LogError("WinnerTeam {Winner} could not be found.", roundResults.RoundWinner);
            return;
        }

        var map = new Map { WinnerTeamName = winnerTeam.TeamConfig.Name, WinnerTeamSide = (TeamSide)(int)winnerTeam.CurrentTeamSide, Name = MatchInfo.CurrentMap.MapName, Team1 = mapTeamInfo1, Team2 = mapTeamInfo2, DemoFileName = Path.GetFileName(MatchInfo.DemoFile) ?? string.Empty };
        _ = _ApiProvider?.RoundStatsUpdateAsync(new RoundStatusUpdateParams(MatchInfo.Config.MatchId, MatchInfo.CurrentMap.MapNumber, teamInfo1, teamInfo2, map, roundResults.Reason, roundResults.RoundTime), CancellationToken.None);
    }

#pragma warning disable MA0051 // Method is too long
    private void UpdateStats(IReadOnlyDictionary<ulong, IPlayerRoundResults> playerResults)
#pragma warning restore MA0051 // Method is too long
    {
        foreach (var kvp in playerResults)
        {
            var steamId = kvp.Key;
            var playerResult = kvp.Value;

            var value = GetOrAddPlayerMatchStatistics(steamId, playerResult);

            var matchStats = value;

            if (playerResult.Dead)
            {
                matchStats.Deaths++;
            }

            if (playerResult.Suicide)
            {
                matchStats.Suicides++;
            }

            if (playerResult.BombDefused)
            {
                matchStats.BombDefuses++;
            }

            if (playerResult.BombPlanted)
            {
                matchStats.BombPlants++;
            }

            if (playerResult.Mvp)
            {
                matchStats.Mvp++;
            }

            if (playerResult.FirstKillCt)
            {
                matchStats.FirstKillCt++;
            }

            if (playerResult.FirstKillT)
            {
                matchStats.FirstKillT++;
            }

            if (playerResult.FirstDeathT)
            {
                matchStats.FirstDeathT++;
            }

            if (playerResult.FirstDeathCt)
            {
                matchStats.FirstDeathCt++;
            }

            matchStats.Kills += playerResult.Kills;
            matchStats.KnifeKills += playerResult.KnifeKills;
            matchStats.TeamKills += playerResult.TeamKills;
            matchStats.EnemiesFlashed += playerResult.EnemiesFlashed;
            matchStats.FlashbangAssists += playerResult.FlashbangAssists;
            matchStats.Assists += playerResult.Assists;
            matchStats.Damage += playerResult.Damage;
            matchStats.HeadshotKills += playerResult.HeadshotKills;
            matchStats.UtilityDamage += playerResult.UtilityDamage;
            matchStats.FriendliesFlashed += playerResult.FriendliesFlashed;
            matchStats.TradeKill += playerResult.TradeKills;

            // Score is the overall value, not reported per round
            matchStats.ContributionScore = playerResult.ContributionScore;

            UpdateKills(playerResult, matchStats);
            UpdateClutchKills(playerResult, matchStats);

            // TODO Kast
        }
    }

    private static void UpdateKills(IPlayerRoundResults playerResult, PlayerMatchStatistics matchStats)
    {
        switch (playerResult.Kills)
        {
            case _Kill1:
                matchStats.Count1K++;
                break;
            case _Kill2:
                matchStats.Count2K++;
                break;
            case _Kill3:
                matchStats.Count3K++;
                break;
            case _Kill4:
                matchStats.Count4K++;
                break;
            case _Kill5:
                matchStats.Count5K++;
                break;
            default:
                // Do nothing
                break;
        }
    }

    private static void UpdateClutchKills(IPlayerRoundResults playerResult, PlayerMatchStatistics matchStats)
    {
        if (playerResult.Clutched)
        {
            switch (playerResult.ClutchKills)
            {
                case _Kill1:
                    matchStats.V1++;
                    break;
                case _Kill2:
                    matchStats.V2++;
                    break;
                case _Kill3:
                    matchStats.V3++;
                    break;
                case _Kill4:
                    matchStats.V4++;
                    break;
                case _Kill5:
                    matchStats.V5++;
                    break;
                default:
                    // Do nothing
                    break;
            }
        }
    }

    private PlayerMatchStatistics GetOrAddPlayerMatchStatistics(ulong steamId, IPlayerRoundResults playerResult)
    {
        if (!MatchInfo.CurrentMap.PlayerMatchStatistics.TryGetValue(steamId, out PlayerMatchStatistics? value))
        {
            value = new PlayerMatchStatistics();
            MatchInfo.CurrentMap.PlayerMatchStatistics[steamId] = value;

            // Set name once
            MatchInfo.CurrentMap.PlayerMatchStatistics[steamId].Name = playerResult.Name;
        }

        return value;
    }

    private static IPlayerStatistics CreatePlayerStatistics(IPlayerMatchStatistics value)
    {
        return new PlayerStatistics
        {
            Assists = value.Assists,
            BombDefuses = value.BombDefuses,
            BombPlants = value.BombPlants,
            Coaching = value.Coaching,
            ContributionScore = value.ContributionScore,
            Count1K = value.Count1K,
            Count2K = value.Count2K,
            Count3K = value.Count3K,
            Count4K = value.Count4K,
            Count5K = value.Count5K,
            Damage = value.Damage,
            Deaths = value.Deaths,
            EnemiesFlashed = value.EnemiesFlashed,
            FirstDeathCt = value.FirstDeathCt,
            FirstDeathT = value.FirstDeathT,
            FirstKillCt = value.FirstKillCt,
            FirstKillT = value.FirstKillT,
            FlashbangAssists = value.FlashbangAssists,
            FriendliesFlashed = value.FriendliesFlashed,
            HeadshotKills = value.HeadshotKills,
            Kast = value.Kast,
            Kills = value.Kills,
            KnifeKills = value.KnifeKills,
            Mvp = value.Mvp,
            Name = value.Name,
            RoundsPlayed = value.RoundsPlayed,
            Suicides = value.Suicides,
            TeamKills = value.TeamKills,
            TradeKill = value.TradeKill,
            UtilityDamage = value.UtilityDamage,
            V1 = value.V1,
            V2 = value.V2,
            V3 = value.V3,
            V4 = value.V4,
            V5 = value.V5,
        };
    }

    private void TryCompleteMatch()
    {
        _ = TryFireStateAsync(MatchCommand.CompleteMatch);
    }



    private async Task CompleteMatchAsync()
    {
        try
        {
            _CsServer.StopDemoRecording();

            int delay = GetSourceTvDelay();

            var seriesResultParams = new SeriesResultParams(MatchInfo.Config.MatchId, MatchInfo.MatchMaps.GroupBy(x => x.Winner).MaxBy(x => x.Count())!.Key!.TeamConfig.Name, Forfeit: true, (uint)delay * 1100, MatchInfo.MatchMaps.Count(x => x.Team1Points > x.Team2Points), MatchInfo.MatchMaps.Count(x => x.Team2Points > x.Team1Points));
            var finalize = _ApiProvider.FinalizeAsync(seriesResultParams, CancellationToken.None);

            while (delay > 0)
            {
                _Logger.LogInformation("Waiting for sourceTV. Remaining Delay: {Delay}s", delay);
                var delayLoopTime = Math.Min(_TimeBetweenDelayMessages, delay);
                await Task.Delay(TimeSpan.FromSeconds(delayLoopTime)).ConfigureAwait(false);
                delay -= delayLoopTime;
            }

            await finalize.ConfigureAwait(false);

            if (_DemoUploader != null)
            {
                await _DemoUploader.UploadDemoAsync(MatchInfo.DemoFile, CancellationToken.None).ConfigureAwait(false);
            }

            _Dispatcher.NextWorldUpdate(() =>
            {
                DoForAll(AllMatchPlayers.ToList(), p => p.Player.Kick());
            });

            await _ApiProvider.FreeServerAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _Logger.LogError(ex, "Unexpected error during finalize.");
        }
        finally
        {
            MatchFinalized?.Invoke(this, new MatchFinalizedEventArgs());
        }
    }

    private int GetSourceTvDelay()
    {
        var delay = 15;

        if (_CsServer.GetConvar<bool>("tv_enable") || _CsServer.GetConvar<bool>("tv_enable1"))
        {
            // TV Delay in s
            var tvDelaySeconds = Math.Max(_CsServer.GetConvar<int>("tv_delay"), _CsServer.GetConvar<int>("tv_delay1"));
            _Logger.LogInformation("Waiting for sourceTV. Delay: {Delay}s + 15s", tvDelaySeconds);
            delay += tvDelaySeconds;
        }

        return delay;
    }

    private void MatchLive()
    {
        _ = Task.Run(async () =>
        {
            var matchIsLiveMessage = _TextHelper.GetText(nameof(Resources.PugSharp_Match_Info_IsLive));
            for (int i = 0; i < _NumOfMatchLiveMessages; i++)
            {
                _CsServer.PrintToChatAll(matchIsLiveMessage);

                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }
        });
    }

    private void OnMatchStateChanged(StateMachine<MatchState, MatchCommand>.Transition transition)
    {
        _Logger.LogInformation("MatchState Changed: {Source} => {Destination}", transition.Source, transition.Destination);
    }

    private void SwitchToMatchMap()
    {
        _CsServer.SwitchMap(MatchInfo.CurrentMap.MapName);
        TryFireState(MatchCommand.SwitchMap);
    }

    private void VoteTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        _VoteTimer.Stop();
        switch (CurrentState)
        {
            case MatchState.MapVote:
                TryFireState(MatchCommand.VoteMap);
                break;
            case MatchState.TeamVote:
                TryFireState(MatchCommand.VoteTeam);
                break;
            default:
                // Do nothing
                break;
        }
    }

    private void ReadyReminderTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (MatchInfo.Config.TeamMode != Config.TeamMode.PlayerSelect && !_ReadyReminderTimer.Enabled)
        {
            return;
        }

        _Dispatcher.NextWorldUpdate(() =>
        {
            try
            {
                if (MatchInfo.Config.TeamMode == Config.TeamMode.PlayerSelect)
                {
                    _Logger.LogInformation("TeamReminder Elapsed");
                    foreach (var player in AllMatchPlayers.Select(p => p.Player))
                    {
                        var matchTeam = GetMatchTeam(player.Team);
                        if (player.Team == Team.Terrorist || player.Team == Team.CounterTerrorist)
                        {
                            var teamMessage = _TextHelper.GetText(nameof(Resources.PugSharp_Match_TeamReminder), matchTeam?.TeamConfig.Name);
                            player.PrintToChat(teamMessage);
                        }
                    }
                }

                if (!_ReadyReminderTimer.Enabled)
                {
                    return;
                }

                _Logger.LogInformation("ReadyReminder Elapsed");
                var readyPlayerIds = AllMatchPlayers.Where(p => p.IsReady).Select(x => x.Player.SteamID).ToList();
                var notReadyPlayers = _CsServer.LoadAllPlayers().Where(p => !readyPlayerIds.Contains(p.SteamID));

                var remindMessage = _TextHelper.GetText(nameof(Resources.PugSharp_Match_RemindReady));
                foreach (var player in notReadyPlayers)
                {
                    player.PrintToChat(remindMessage);
                    player.Clan = _TextHelper.GetText(nameof(Resources.PugSharp_Match_NotReadyTag));
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error sending vote reminder");
            }
        });
    }

    public string CreateDotGraph()
    {
        return UmlDotGraph.Format(_MatchStateMachine.GetInfo());
    }

    private void InitializeMapsToVote(StateMachine<MatchState, MatchCommand>.Transition transition)
    {
        if (transition.Source == MatchState.DefineTeams)
        {
            var playedMaps = MatchInfo.MatchMaps.Select(x => x.MapName).Where(x => !string.IsNullOrEmpty(x));
            _MapsToSelect = MatchInfo.Config.Maplist.Except(playedMaps!, StringComparer.Ordinal).Select(x => new Vote(x)).ToList();
        }
    }

    private void SendRemainingMapsToVotingTeam()
    {
        if (_MapsToSelect == null)
        {
            _Logger.LogError("There are no maps configured! Map Selection is not possible!");
            return;
        }

        // If only one map is configured
        if (MatchInfo.Config.Maplist.Count == 1)
        {
            _MapsToSelect = MatchInfo.Config.Maplist.Select(x => new Vote(x)).ToList();
            TryFireState(MatchCommand.VoteMap);
            return;
        }

        SwitchVotingTeam();

        _MapsToSelect.ForEach(m => m.Votes.Clear());

        var mapOptions = new List<MenuOption>();

        for (int i = 0; i < _MapsToSelect.Count; i++)
        {
            var mapNumber = i;
            string? map = _MapsToSelect[mapNumber].Name;
            mapOptions.Add(new MenuOption(map, (opt, player) => BanMap(player, mapNumber)));
        }


        ShowMenuToTeam(_CurrentMatchTeamToVote!, _TextHelper.GetText(nameof(Resources.PugSharp_Match_VoteMapMenuHeader)), mapOptions);

        DoForAll(_CurrentMatchTeamToVote!.Players.Select(x => x.Player), p => p.Clan = _TextHelper.GetText(nameof(Resources.PugSharp_Match_VotingTag)));
        DoForAll(GetOtherTeam(_CurrentMatchTeamToVote).Players.Select(x => x.Player), p => p.Clan = string.Empty);

        GetOtherTeam(_CurrentMatchTeamToVote!).PrintToChat(_TextHelper.GetText(nameof(Resources.PugSharp_Match_WaitForOtherTeam)));

        _VoteTimer.Start();
    }

    private void RemoveBannedMap()
    {
        if (_VoteTimer.Enabled)
        {
            _VoteTimer.Stop();
        }

        if (_MapsToSelect == null)
        {
            return;
        }

        var team = _CurrentMatchTeamToVote == null || MatchInfo.Config.Team1 == _CurrentMatchTeamToVote.TeamConfig ? "team1" : "team2";

        //Only ban map if there is more than one
        if (_MapsToSelect.Count > 1)
        {
            var mapToBan = _MapsToSelect.MaxBy(m => m.Votes.Count);
            _MapsToSelect.Remove(mapToBan!);
            _MapsToSelect.ForEach(x => x.Votes.Clear());

            _CsServer.PrintToChatAll(_TextHelper.GetText(nameof(Resources.PugSharp_Match_BannedMap), mapToBan!.Name, _CurrentMatchTeamToVote!.TeamConfig.Name));
            _ = _ApiProvider.MapVetoedAsync(new MapVetoedParams(MatchInfo.Config.MatchId, mapToBan.Name, team), CancellationToken.None);
        }

        if (_MapsToSelect.Count == 1)
        {
            MatchInfo.CurrentMap.MapName = _MapsToSelect[0].Name;
            _MapsToSelect = MatchInfo.Config.Maplist.Select(x => new Vote(x)).ToList();
            _ = _ApiProvider.MapPickedAsync(new MapPickedParams(MatchInfo.Config.MatchId, MatchInfo.CurrentMap.MapName, 1, team), CancellationToken.None);
        }
    }

    private void SendTeamVoteToVotingTeam()
    {
        SwitchVotingTeam();

        var mapOptions = new List<MenuOption>()
        {
            new("T", (opt, player) => VoteTeam(player, "T")),
            new("CT", (opt, player) => VoteTeam(player, "CT")),
        };

        ShowMenuToTeam(_CurrentMatchTeamToVote!, _TextHelper.GetText(nameof(Resources.PugSharp_Match_VoteTeamMenuHeader)), mapOptions);
        GetOtherTeam(_CurrentMatchTeamToVote!).PrintToChat(_TextHelper.GetText(nameof(Resources.PugSharp_Match_WaitForOtherTeam)));

        DoForAll(_CurrentMatchTeamToVote!.Players.Select(x => x.Player), p => p.Clan = _TextHelper.GetText(nameof(Resources.PugSharp_Match_VotingTag)));
        DoForAll(GetOtherTeam(_CurrentMatchTeamToVote).Players.Select(x => x.Player), p => p.Clan = string.Empty);

        _VoteTimer.Start();
    }

    private void SetSelectedTeamSide()
    {
        _VoteTimer.Stop();

        var startTeam = _TeamVotes.MaxBy(m => m.Votes.Count)!.Name.Equals("T", StringComparison.OrdinalIgnoreCase) ? Team.Terrorist : Team.CounterTerrorist;
        _Logger.LogInformation("Set selected teamsite to {StartTeam}. Voted by {Team}", startTeam, _CurrentMatchTeamToVote!.TeamConfig.Name);

        if (_CurrentMatchTeamToVote!.CurrentTeamSide != startTeam)
        {
            _CurrentMatchTeamToVote.StartingTeamSide = startTeam;
            _CurrentMatchTeamToVote.CurrentTeamSide = startTeam;
            var otherTeam = GetOtherTeam(_CurrentMatchTeamToVote);
            otherTeam.StartingTeamSide = startTeam == Team.Terrorist ? Team.CounterTerrorist : Team.Terrorist;
            otherTeam.CurrentTeamSide = otherTeam.StartingTeamSide;

            _Logger.LogInformation("{Team} starts as Team {StartTeam}", _CurrentMatchTeamToVote.TeamConfig.Name, _CurrentMatchTeamToVote!.CurrentTeamSide.ToString());
            _Logger.LogInformation("{Team} starts as Team {StartTeam}", otherTeam.TeamConfig.Name, otherTeam!.CurrentTeamSide.ToString());
        }

        _CsServer.PrintToChatAll(_TextHelper.GetText(nameof(Resources.PugSharp_Match_SelectedTeam), _CurrentMatchTeamToVote!.TeamConfig.Name, startTeam));
    }

    public MatchTeam GetOtherTeam(MatchTeam team)
    {
        return team == MatchInfo.MatchTeam1 ? MatchInfo.MatchTeam2 : MatchInfo.MatchTeam1;
    }

    private void ShowMenuToTeam(MatchTeam team, string title, IEnumerable<MenuOption> options)
    {
        DoForAll(team.Players.Select(x => x.Player), p => p.ShowMenu(title, options));
    }

    private void SwitchVotingTeam()
    {
        if (_CurrentMatchTeamToVote == null)
        {
            _CurrentMatchTeamToVote = MatchInfo.MatchTeam1;
        }
        else
        {
            _CurrentMatchTeamToVote = _CurrentMatchTeamToVote == MatchInfo.MatchTeam1 ? MatchInfo.MatchTeam2 : MatchInfo.MatchTeam1;
        }
    }

    private bool AllPlayersAreConnected()
    {
        var players = _CsServer.LoadAllPlayers();
        var connectedPlayerSteamIds = players.Select(p => p.SteamID).ToList();
        var allPlayerIds = MatchInfo.Config.Team1.Players.Keys.Concat(MatchInfo.Config.Team2.Players.Keys);
        if (allPlayerIds.All(p => connectedPlayerSteamIds.Contains(p)))
        {
            return true;
        }

        return false;
    }

    private bool AllPlayersAreReady()
    {
        var readyPlayers = AllMatchPlayers.Where(p => p.IsReady);
        var requiredPlayers = MatchInfo.Config.MinPlayersToReady * 2;

        return readyPlayers.Take(requiredPlayers + 1).Count() == requiredPlayers;
    }

    private bool AllTeamsUnpaused() => !MatchInfo.MatchTeam1.IsPaused && !MatchInfo.MatchTeam2.IsPaused;

    private bool NotAllMapsArePlayed() => !AllMapsArePlayed();

    private bool AllMapsArePlayed()
    {
        var teamWithMostWins = MatchInfo.MatchMaps.Where(x => x.Winner != null).GroupBy(x => x.Winner).MaxBy(x => x.Count());
        if (teamWithMostWins?.Key == null)
        {
            _Logger.LogError("Can not check if all maps are ready. No team with wins!");
            return false;
        }

        var wins = teamWithMostWins.Count();
        var requiredWins = Math.Ceiling(MatchInfo.Config.NumMaps / 2d);
        return wins >= requiredWins;
    }

    private void SetAllPlayersNotReady()
    {
        _Logger.LogInformation("Reset Readystate for all players");

        foreach (var player in AllMatchPlayers)
        {
            player.IsReady = false;
        }

        _CsServer.PrintToChatAll(_TextHelper.GetText(nameof(Resources.PugSharp_Match_Info_WaitingForAllPlayers)));
        _CsServer.PrintToChatAll(_TextHelper.GetText(nameof(Resources.PugSharp_Match_RemindReady)));
    }

    private bool MapIsSelected()
    {
        // The SelectedCount is checked when the Votes are done but the map is still in the list
        return _MapsToSelect.Count <= 2;
    }

    private bool MapIsNotSelected()
    {
        return !MapIsSelected();
    }

    private bool TryFireState(MatchCommand command)
    {
        if (_MatchStateMachine.CanFire(command))
        {
            _MatchStateMachine.Fire(command);
            return true;
        }

        return false;
    }

    private async Task TryFireStateAsync(MatchCommand command)
    {
        try
        {
            if (_MatchStateMachine.CanFire(command))
            {
                await _MatchStateMachine.FireAsync(command).ConfigureAwait(false);
            }
        }
        catch (Exception exception)
        {
            _Logger.LogError(exception, "Error during fire state");
        }
    }

    private MatchTeam? GetMatchTeam(ulong steamID)
    {
        _Logger.LogInformation("GetMatchTeam for {SteamId} in MatchTeam1: {Team1Ids}", steamID, string.Join(", ", MatchInfo.MatchTeam1.Players.Select(x => x.Player.SteamID)));
        if (MatchInfo.MatchTeam1.Players.Any(x => x.Player.SteamID.Equals(steamID)))
        {
            return MatchInfo.MatchTeam1;
        }

        _Logger.LogInformation("GetMatchTeam for {SteamId} in MatchTeam2: {Team1Ids}", steamID, string.Join(", ", MatchInfo.MatchTeam2.Players.Select(x => x.Player.SteamID)));
        if (MatchInfo.MatchTeam2.Players.Any(x => x.Player.SteamID.Equals(steamID)))
        {
            return MatchInfo.MatchTeam2;
        }

        return null;
    }

    private MatchTeam? GetMatchTeam(Team team)
    {
        return MatchInfo.MatchTeam1.CurrentTeamSide == team ? MatchInfo.MatchTeam1 : MatchInfo.MatchTeam2;
    }

    private MatchPlayer GetMatchPlayer(ulong steamID)
    {
        return AllMatchPlayers.First(x => x.Player.SteamID == steamID);
    }

    public bool PlayerIsReady(ulong steamID)
    {
        var matchPlayer = AllMatchPlayers.FirstOrDefault(x => x.Player.SteamID == steamID);
        return matchPlayer != null && matchPlayer.IsReady;
    }

    #region Match Functions

    private bool TryAddPlayerToCurrentTeam(IPlayer player)
    {
        var matchPlayer = MatchInfo.MatchTeam1.Players.Any(x => x.Player.SteamID == player.SteamID) || MatchInfo.MatchTeam2.Players.Any(x => x.Player.SteamID == player.SteamID) ? GetMatchPlayer(player.SteamID) : null;
        if (matchPlayer == null)
        {
            matchPlayer = new MatchPlayer(player);
        }
        else
        {
            // Remove the player from the match
            MatchInfo.MatchTeam1.Players.Remove(matchPlayer);
            MatchInfo.MatchTeam2.Players.Remove(matchPlayer);
        }

        // Add them back to the match
        bool isTeam1;
        if (MatchInfo.MatchTeam1.CurrentTeamSide == Team.None && MatchInfo.MatchTeam2.CurrentTeamSide == Team.None)
        {
            isTeam1 = player.Team == Team.Terrorist;
        }
        else if (MatchInfo.MatchTeam1.CurrentTeamSide != Team.None)
        {
            isTeam1 = MatchInfo.MatchTeam1.CurrentTeamSide == player.Team;
        }
        else if (MatchInfo.MatchTeam2.CurrentTeamSide != Team.None)
        {
            isTeam1 = MatchInfo.MatchTeam2.CurrentTeamSide != player.Team;
        }
        else
        {
            _Logger.LogInformation("Could not add player {SteamId} to match", player.SteamID);
            return false;
        }

        if (isTeam1)
        {
            MatchInfo.MatchTeam1.Players.Add(matchPlayer);
        }
        else
        {
            MatchInfo.MatchTeam2.Players.Add(matchPlayer);
        }

        return true;
    }

    public bool TryAddPlayer(IPlayer player)
    {
        if (!PlayerBelongsToMatch(player.SteamID))
        {
            _Logger.LogInformation("Player with steam id {SteamId} is no member of this match!", player.SteamID);
            return false;
        }

        if (MatchInfo.Config.TeamMode == Config.TeamMode.PlayerSelect)
        {
            // Quicker to just remove them and add them back, rather than check whether they are already in the match
            return TryAddPlayerToCurrentTeam(player);
        }

        if (MatchInfo.MatchTeam1.Players.Any(x => x.Player.SteamID == player.SteamID)
            || MatchInfo.MatchTeam2.Players.Any(x => x.Player.SteamID == player.SteamID))
        {
            // Player is already part of this match
            _ = TryFireStateAsync(MatchCommand.ConnectPlayer);
            return true;
        }

        var isTeam1 = MatchInfo.Config.Team1.Players.ContainsKey(player.SteamID);
        var isTeam2 = !isTeam1 && MatchInfo.Config.Team2.Players.ContainsKey(player.SteamID);
        if (MatchInfo.RandomPlayersAllowed && !isTeam1 && !isTeam2)
        {
            // if no team is configured add player to team with less players
            isTeam1 = MatchInfo.MatchTeam1.Players.Count <= MatchInfo.MatchTeam2.Players.Count;
            if (isTeam1)
            {
                MatchInfo.Config.Team1.Players.Add(player.SteamID, player.PlayerName);
            }
            else
            {
                MatchInfo.Config.Team2.Players.Add(player.SteamID, player.PlayerName);
            }
        }

        var team = isTeam1 ? MatchInfo.MatchTeam1 : MatchInfo.MatchTeam2;
        var startSide = team.CurrentTeamSide;
        if (startSide == Team.None)
        {
            startSide = isTeam1 ? Team.Terrorist : Team.CounterTerrorist;
        }

        _Logger.LogInformation("Player {PlayerName} belongs to {TeamName}", player.PlayerName, team.TeamConfig.Name);

        if (player.Team != startSide)
        {
            _Logger.LogInformation("Player {PlayerName} should be on {StartSide} but is {CurrentTeam}", player.PlayerName, startSide, player.Team);

            player.SwitchTeam(startSide);
        }

        team.Players.Add(new MatchPlayer(player));

        _ = TryFireStateAsync(MatchCommand.ConnectPlayer);
        return true;
    }

    public void SetPlayerDisconnected(IPlayer player)
    {
        var matchTeam = GetMatchTeam(player.SteamID);
        if (matchTeam == null)
        {
            return;
        }

        var matchPlayer = GetMatchPlayer(player.SteamID);
        TryFireState(MatchCommand.DisconnectPlayer);

        switch (CurrentState)
        {
            case MatchState.WaitingForPlayersConnectedReady:
                matchPlayer.IsReady = false;
                break;
            case MatchState.MapVote:
            case MatchState.TeamVote:
            case MatchState.SwitchMap:
            case MatchState.WaitingForPlayersReady:
            case MatchState.MatchStarting:
                break;
            case MatchState.MatchRunning:
                Pause(player);
                break;
            case MatchState.MatchPaused:
            case MatchState.MatchCompleted:
                break;
            default:
                matchTeam.Players.Remove(matchPlayer);
                break;
        }
    }

    public void TogglePlayerIsReady(IPlayer player)
    {
        if (CurrentState != MatchState.WaitingForPlayersConnectedReady && CurrentState != MatchState.WaitingForPlayersReady)
        {
            player.PrintToChat(_TextHelper.GetText(nameof(Resources.PugSharp_Match_Error_NoReadyExpected)));
            return;
        }

        var matchPlayer = GetMatchPlayer(player.SteamID);
        if (matchPlayer.Player.Team != Team.Terrorist && matchPlayer.Player.Team != Team.CounterTerrorist)
        {
            player.PrintToChat(_TextHelper.GetText(nameof(Resources.PugSharp_Match_Error_NoReadyExpected)));
            return;
        }

        matchPlayer.IsReady = !matchPlayer.IsReady;

        var readyPlayers = MatchInfo.MatchTeam1.Players.Count(x => x.IsReady) + MatchInfo.MatchTeam2.Players.Count(x => x.IsReady);

        // Min Players per Team
        var requiredPlayers = MatchInfo.Config.MinPlayersToReady * 2;

        if (matchPlayer.IsReady)
        {
            player.Clan = _TextHelper.GetText(nameof(Resources.PugSharp_Match_ReadyTag));
            _CsServer.PrintToChatAll(_TextHelper.GetText(nameof(Resources.PugSharp_Match_Info_Ready), player.PlayerName, readyPlayers, requiredPlayers));
            TryFireState(MatchCommand.PlayerReady);
        }
        else
        {
            player.Clan = _TextHelper.GetText(nameof(Resources.PugSharp_Match_NotReadyTag));
            _CsServer.PrintToChatAll(_TextHelper.GetText(nameof(Resources.PugSharp_Match_Info_NotReady), player.PlayerName, readyPlayers, requiredPlayers));
        }

    }

    public Team GetPlayerTeam(ulong steamID)
    {
        var matchTeam = GetMatchTeam(steamID);

        if (matchTeam != null)
        {
            return matchTeam.CurrentTeamSide;
        }

        _Logger.LogInformation("No matchTeam found. Fallback to Config Team!");
        return GetConfigTeam(steamID);
    }

    private Team GetConfigTeam(ulong steamID)
    {
        if (MatchInfo.Config.Team1.Players.ContainsKey(steamID))
        {
            return Team.Terrorist;
        }

        if (MatchInfo.Config.Team2.Players.ContainsKey(steamID))
        {
            return Team.CounterTerrorist;
        }

        if (MatchInfo.Config.Team1.Players.Count == 0 && MatchInfo.Config.Team2.Players.Count == 0)
        {
            return MatchInfo.MatchTeam1.Players.Count < MatchInfo.MatchTeam2.Players.Count ? Team.Terrorist : Team.CounterTerrorist;
        }

        return Team.None;
    }

    public bool BanMap(IPlayer player, int mapNumber)
    {
        if (CurrentState != MatchState.MapVote)
        {
            player.PrintToChat(_TextHelper.GetText(nameof(Resources.PugSharp_Match_Error_NoMapVoteExpected)));
            return false;
        }

        if (_CurrentMatchTeamToVote == null)
        {
            // Should never happen
            return false;
        }

        if (!_CurrentMatchTeamToVote.Players.Select(x => x.Player.UserId).Contains(player.UserId))
        {
            player.PrintToChat(_TextHelper.GetText(nameof(Resources.PugSharp_Match_Error_NotPermittedToBanMap)));
            return false;
        }


        if (_MapsToSelect.Count <= mapNumber || mapNumber < 0)
        {
            player.PrintToChat(_TextHelper.GetText(nameof(Resources.PugSharp_Match_Error_UnknownMapNumber), mapNumber));
            return false;
        }

        var bannedMap = _MapsToSelect.Find(x => x.Votes.Any(x => x.UserId == player.UserId));
        if (bannedMap != null)
        {
            player.PrintToChat(_TextHelper.GetText(nameof(Resources.PugSharp_Match_Error_AlreadyBannedMap), _MapsToSelect.IndexOf(bannedMap), bannedMap.Name));
            return false;
        }

        var mapToSelect = _MapsToSelect[mapNumber];
        mapToSelect.Votes.Add(player);

        player.PrintToChat(_TextHelper.GetText(nameof(Resources.PugSharp_Match_VotedToBanMap), mapToSelect.Name));
        player.Clan = string.Empty;

        if (_MapsToSelect.Sum(x => x.Votes.Count) >= MatchInfo.Config.PlayersPerTeam)
        {
            TryFireState(MatchCommand.VoteMap);
        }

        return true;
    }

    public bool VoteTeam(IPlayer player, string teamName)
    {
        // Not in correct state
        if (CurrentState != MatchState.TeamVote)
        {
            player.PrintToChat(_TextHelper.GetText(nameof(Resources.PugSharp_Match_Error_NoTeamVoteExpected)));
            return false;
        }

        if (_CurrentMatchTeamToVote == null)
        {
            // Should never happen
            return false;
        }

        // Player not permitted to vote for this team
        if (!_CurrentMatchTeamToVote.Players.Select(x => x.Player.UserId).Contains(player.UserId))
        {
            player.PrintToChat(_TextHelper.GetText(nameof(Resources.PugSharp_Match_Error_NotPermittedToVoteForTeam)));
            return false;
        }

        // Player already voted for this team
        var votedTeam = _TeamVotes.Find(x => x.Votes.Any(x => x.UserId == player.UserId));
        if (votedTeam != null)
        {
            player.PrintToChat(_TextHelper.GetText(nameof(Resources.PugSharp_Match_Error_AlreadyVotedForTeam), _MapsToSelect.IndexOf(votedTeam), votedTeam.Name));
            return false;
        }

        // No team found
        var teamToVote = _TeamVotes.Find(x => x.Name.Equals(teamName, StringComparison.OrdinalIgnoreCase));
        if (teamToVote == null)
        {
            player.PrintToChat(_TextHelper.GetText(nameof(Resources.PugSharp_Match_Error_TeamNotAvailable), teamName));
            return false;
        }

        teamToVote.Votes.Add(player);

        // Successful vote
        player.PrintToChat(_TextHelper.GetText(nameof(Resources.PugSharp_Match_VotedForTeam), teamToVote.Name));
        player.Clan = string.Empty;

        if (_TeamVotes.Sum(x => x.Votes.Count) >= MatchInfo.Config.PlayersPerTeam)
        {
            TryFireState(MatchCommand.VoteTeam);
        }

        return true;
    }

    public void Pause(IPlayer player)
    {
        if (!TryFireState(MatchCommand.Pause))
        {
            player.PrintToChat(_TextHelper.GetText(nameof(Resources.PugSharp_Match_Error_PauseNotPossible)));
        }
    }

    public void Unpause(IPlayer player)
    {
        var team = GetMatchTeam(player.SteamID);
        if (team == null)
        {
            player.PrintToChat(_TextHelper.GetText(nameof(Resources.PugSharp_Match_Error_UnpauseNotPossible)));
            return;
        }

        team.IsPaused = false;
        TryFireState(MatchCommand.Unpause);
    }

    public void SwitchTeam()
    {
        _Logger.LogInformation("Toggle TeamSides");

        MatchInfo.MatchTeam1.ToggleTeamSide();
        MatchInfo.MatchTeam2.ToggleTeamSide();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_DisposedValue)
        {
            if (disposing)
            {
                _VoteTimer.Elapsed -= VoteTimer_Elapsed;
                _VoteTimer.Dispose();

                _ReadyReminderTimer.Elapsed -= ReadyReminderTimer_Elapsed;
                _ReadyReminderTimer.Dispose();
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

    public void CompleteMap(int tPoints, int ctPoints)
    {
        int delay = GetSourceTvDelay();
        _CsServer.UpdateConvar("mp_win_panel_display_time", (float)delay);

        var winner = tPoints > ctPoints ? Team.Terrorist : Team.CounterTerrorist;

        var winnerTeam = GetMatchTeam(winner) ?? throw new NotSupportedException("Winner Team could not be found!");
        MatchInfo.CurrentMap.Winner = winnerTeam;

        var configWinnerTeam = GetConfigTeam(winnerTeam.Players[0].Player.SteamID);
        if (configWinnerTeam == Team.Terrorist)
        {
            // Team 1 won
            MatchInfo.CurrentMap.Team1Points = winner == Team.Terrorist ? tPoints : ctPoints;
            MatchInfo.CurrentMap.Team2Points = winner == Team.Terrorist ? ctPoints : tPoints;
        }
        else
        {
            // Team 2 won
            MatchInfo.CurrentMap.Team1Points = winner == Team.Terrorist ? ctPoints : tPoints;
            MatchInfo.CurrentMap.Team2Points = winner == Team.Terrorist ? tPoints : ctPoints;
        }

        _Logger.LogInformation("The winner is: {Winner}", winnerTeam!.TeamConfig.Name);
        _ = TryFireStateAsync(MatchCommand.CompleteMap);
    }

    public bool PlayerBelongsToMatch(ulong steamId)
    {
        if (MatchInfo.RandomPlayersAllowed)
        {
            // Allow matches without player configuration wait for the first 10 players
            return true;
        }

        return MatchInfo.Config.Team1.Players.Any(x => x.Key.Equals(steamId))
                || MatchInfo.Config.Team2.Players.Any(x => x.Key.Equals(steamId));
    }

    private void DoForAll<T>(IEnumerable<T> items, Action<T> action)
    {
        foreach (var item in items)
        {
            try
            {
                action(item);
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "Error executing action!");
            }
        }
    }

    #endregion
}
