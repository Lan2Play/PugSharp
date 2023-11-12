using Microsoft.Extensions.Logging;
using PugSharp.Api.Contract;
using PugSharp.ApiStats;
using PugSharp.Logging;
using PugSharp.Match.Contract;
using Stateless;
using Stateless.Graph;
using System.Globalization;

namespace PugSharp.Match;

public class Match : IDisposable
{
    private const int Kill1 = 1;
    private const int Kill2 = 2;
    private const int Kill3 = 3;
    private const int Kill4 = 4;
    private const int Kill5 = 5;
    private const int NumOfMatchLiveMessages = 10;

    private static readonly ILogger<Match> _Logger = LogManager.CreateLogger<Match>();

    private readonly System.Timers.Timer _VoteTimer = new();
    private readonly System.Timers.Timer _ReadyReminderTimer = new(10000);
    private readonly IMatchCallback _MatchCallback;
    private readonly IApiProvider _ApiProvider;
    private readonly string _RoundBackupFile;
    private readonly StateMachine<MatchState, MatchCommand> _MatchStateMachine;

    private readonly DemoUploader? _DemoUploader;
    private readonly List<Vote> _TeamVotes = new() { new("T"), new("CT") };

    private List<Vote> _MapsToSelect;
    private MatchTeam? _CurrentMatchTeamToVote;
    private bool disposedValue;

    public MatchState CurrentState => _MatchStateMachine.State;

    public MatchInfo MatchInfo { get; }

    public MatchTeam MatchTeam1 { get; }

    public MatchTeam MatchTeam2 { get; }

    public IEnumerable<MatchPlayer> AllMatchPlayers => MatchTeam1.Players.Concat(MatchTeam2.Players);

    public Match(IMatchCallback matchCallback, IApiProvider apiProvider, Config.MatchConfig matchConfig)
    {
        _MatchCallback = matchCallback;
        _ApiProvider = apiProvider;
        MatchInfo = new MatchInfo(matchConfig);
        MatchTeam1 = new MatchTeam(MatchInfo.Config.Team1);
        MatchTeam2 = new MatchTeam(MatchInfo.Config.Team2);

        _VoteTimer.Interval = MatchInfo.Config.VoteTimeout;
        _VoteTimer.Elapsed += VoteTimer_Elapsed;
        _ReadyReminderTimer.Elapsed += ReadyReminderTimer_Elapsed;

        if (!string.IsNullOrEmpty(MatchInfo.Config.EventulaDemoUploadUrl) && !string.IsNullOrEmpty(MatchInfo.Config.EventulaApistatsToken))
        {
            _DemoUploader = new DemoUploader(MatchInfo.Config.EventulaDemoUploadUrl, MatchInfo.Config.EventulaApistatsToken);
        }

        _MapsToSelect = MatchInfo.Config.Maplist.Select(x => new Vote(x)).ToList();

        _MatchStateMachine = new StateMachine<MatchState, MatchCommand>(MatchState.None);
        InitializeStateMachine();

        SetMatchTeamCvars();
    }

    public Match(IMatchCallback matchCallback, IApiProvider apiProvider, MatchInfo matchInfo, string roundBackupFile)
    {
        _Logger.LogInformation("Create Match from existing MatchInfo!");
        _MatchCallback = matchCallback;
        _ApiProvider = apiProvider;
        _RoundBackupFile = roundBackupFile;
        MatchInfo = matchInfo;
        // TODO CompleteMatch if alls maps have an winner?
        MatchInfo.CurrentMap = matchInfo.MatchMaps.FirstOrDefault(x => x.Winner == null) ?? matchInfo.MatchMaps.Last();
        _Logger.LogInformation("Continue Match on map {mapNumber}({mapName})!", MatchInfo.CurrentMap.MapNumber, MatchInfo.CurrentMap.MapName);
        MatchTeam1 = new MatchTeam(MatchInfo.Config.Team1);
        MatchTeam2 = new MatchTeam(MatchInfo.Config.Team2);

        _VoteTimer.Interval = MatchInfo.Config.VoteTimeout;
        _VoteTimer.Elapsed += VoteTimer_Elapsed;
        _ReadyReminderTimer.Elapsed += ReadyReminderTimer_Elapsed;

        if (!string.IsNullOrEmpty(MatchInfo.Config.EventulaDemoUploadUrl) && !string.IsNullOrEmpty(MatchInfo.Config.EventulaApistatsToken))
        {
            _DemoUploader = new DemoUploader(MatchInfo.Config.EventulaDemoUploadUrl, MatchInfo.Config.EventulaApistatsToken);
        }

        _MapsToSelect = MatchInfo.Config.Maplist.Select(x => new Vote(x)).ToList();

        _MatchStateMachine = new StateMachine<MatchState, MatchCommand>(MatchState.None);
        InitializeStateMachine();

        SetMatchTeamCvars();
    }

#pragma warning disable MA0051 // Method is too long
    private void InitializeStateMachine()
#pragma warning restore MA0051 // Method is too long
    {
        _MatchStateMachine.Configure(MatchState.None)
            .PermitIf(MatchCommand.LoadMatch, MatchState.WaitingForPlayersConnectedReady, HasNoRestoredMatch)
            .PermitIf(MatchCommand.LoadMatch, MatchState.WaitingForPlayersReady, HasRestoredMatch);

        _MatchStateMachine.Configure(MatchState.WaitingForPlayersConnectedReady)
            .PermitIf(MatchCommand.PlayerReady, MatchState.MapVote, AllPlayersAreReady)
            .OnEntry(SetAllPlayersNotReady)
            .OnEntry(StartReadyReminder)
            .OnExit(StopReadyReminder);

        _MatchStateMachine.Configure(MatchState.MapVote)
            .PermitReentryIf(MatchCommand.VoteMap, MapIsNotSelected)
            .PermitIf(MatchCommand.VoteMap, MatchState.TeamVote, MapIsSelected)
            .OnEntry(SendRemainingMapsToVotingTeam)
            .OnExit(RemoveBannedMap);

        _MatchStateMachine.Configure(MatchState.TeamVote)
            .Permit(MatchCommand.VoteTeam, MatchState.SwitchMap)
            .OnEntry(SendTeamVoteToVotingteam)
            .OnExit(SetSelectedTeamSite);

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
            .OnEntryAsync(MatchLiveAsync);

        _MatchStateMachine.Configure(MatchState.MatchPaused)
            .PermitIf(MatchCommand.ConnectPlayer, MatchState.MatchRunning, AllPlayersAreConnected)
            .PermitIf(MatchCommand.Unpause, MatchState.MatchRunning, AllTeamsUnpaused)
            .OnEntry(PauseMatch)
            .OnExit(UnpauseMatch);

        _MatchStateMachine.Configure(MatchState.MapCompleted)
            .PermitIf(MatchCommand.CompleteMatch, MatchState.MatchCompleted, AllMapsArePlayed)
            .PermitIf(MatchCommand.CompleteMatch, MatchState.WaitingForPlayersConnectedReady, NotAllMapsArePlayed)
            .OnEntry(SendMapResults)
            .OnEntry(TryCompleteMatch);

        _MatchStateMachine.Configure(MatchState.MatchCompleted)
            .PermitIf(MatchCommand.CleanUpMatch, MatchState.CleanUpMatch)
            .OnEntry(CleanUpMatch);


        _MatchStateMachine.Configure(MatchState.CleanUpMatch)
            .OnEntryAsync(CompleteMatchAsync);

        _MatchStateMachine.OnTransitioned(OnMatchStateChanged);

        _MatchStateMachine.Fire(MatchCommand.LoadMatch);
    }

    private bool HasNoRestoredMatch()
    {
        return !HasRestoredMatch();
    }

    private bool HasRestoredMatch()
    {
        return !string.IsNullOrEmpty(MatchInfo.CurrentMap.MapName);
    }

    private void SetMatchTeamCvars()
    {

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
    }

    private void UnpauseMatch()
    {
        _MatchCallback.UnpauseMatch();
    }

    private void PauseMatch()
    {
        MatchTeam1.IsPaused = true;
        MatchTeam2.IsPaused = true;

        _MatchCallback.PauseMatch();
    }

    private void StartMatch()
    {
        _MatchCallback.RestoreBackup(_RoundBackupFile);
        _MatchCallback.EndWarmup();
        _MatchCallback.DisableCheats();
        _MatchCallback.SetupRoundBackup();
        MatchInfo.DemoFile = _MatchCallback.StartDemoRecording();

        _MatchCallback.SendMessage(string.Create(CultureInfo.InvariantCulture, $" {ChatColors.Default}Starting Match. {ChatColors.Highlight}{MatchTeam1.TeamConfig.Name} {ChatColors.Default}as {ChatColors.Highlight}{MatchTeam1.CurrentTeamSite}{ChatColors.Default}. {ChatColors.Highlight}{MatchTeam2.TeamConfig.Name}{ChatColors.Default} as {ChatColors.Highlight}{MatchTeam2.CurrentTeamSite}"));

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
            TeamName = MatchInfo.Config.Team1.Name,
        };

        var teamInfo2 = new TeamInfo
        {
            TeamName = MatchInfo.Config.Team2.Name,
        };

        UpdateStats(roundResults.PlayerResults);

        var team1Results = MatchTeam1.CurrentTeamSite == Team.Terrorist ? roundResults.TRoundResult : roundResults.CTRoundResult;
        var team2Results = MatchTeam2.CurrentTeamSite == Team.Terrorist ? roundResults.TRoundResult : roundResults.CTRoundResult;

        _Logger.LogInformation("Team 1: {teamSite} : {teamScore}", MatchTeam1.CurrentTeamSite, team1Results.Score);
        _Logger.LogInformation("Team 2: {teamSite} : {teamScore}", MatchTeam2.CurrentTeamSite, team2Results.Score);

        var mapTeamInfo1 = new MapTeamInfo
        {
            StartingSide = MatchTeam1.StartingTeamSite == Team.Terrorist ? StartingSide.T : StartingSide.CT,
            Score = team1Results.Score,
            ScoreT = team1Results.ScoreT,
            ScoreCT = team1Results.ScoreCT,
            Players = MatchInfo.CurrentMap.PlayerMatchStatistics
                            .Where(a => MatchTeam1.Players.Select(player => player.Player.SteamID).Contains(a.Key))
                            .ToDictionary(p => p.Key.ToString(CultureInfo.InvariantCulture), p => CreatePlayerStatistics(p.Value), StringComparer.OrdinalIgnoreCase),
        };

        var mapTeamInfo2 = new MapTeamInfo
        {
            StartingSide = MatchTeam2.StartingTeamSite == Team.Terrorist ? StartingSide.T : StartingSide.CT,
            Score = team2Results.Score,
            ScoreT = team2Results.ScoreT,
            ScoreCT = team2Results.ScoreCT,

            Players = MatchInfo.CurrentMap.PlayerMatchStatistics
                            .Where(a => MatchTeam2.Players.Select(player => player.Player.SteamID).Contains(a.Key))
                            .ToDictionary(p => p.Key.ToString(CultureInfo.InvariantCulture), p => CreatePlayerStatistics(p.Value), StringComparer.OrdinalIgnoreCase),
        };

        var winnerTeam = GetMatchTeam(roundResults.RoundWinner);
        if (winnerTeam == null)
        {
            _Logger.LogError("WinnerTeam {winner} could not be found.", roundResults.RoundWinner);
            return;
        }

        var map = new Map { WinnerTeamName = winnerTeam.TeamConfig.Name, Name = MatchInfo.CurrentMap.MapName, Team1 = mapTeamInfo1, Team2 = mapTeamInfo2, DemoFileName = Path.GetFileName(MatchInfo.DemoFile) };
        _ = _ApiProvider?.RoundStatsUpdateAsync(new RoundStatusUpdateParams(MatchInfo.Config.MatchId, MatchInfo.CurrentMap.MapNumber, teamInfo1, teamInfo2, map), CancellationToken.None);
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

            switch (playerResult.Kills)
            {
                case Kill1:
                    matchStats.Count1K++;
                    break;
                case Kill2:
                    matchStats.Count2K++;
                    break;
                case Kill3:
                    matchStats.Count3K++;
                    break;
                case Kill4:
                    matchStats.Count4K++;
                    break;
                case Kill5:
                    matchStats.Count5K++;
                    break;
                default:
                    // Do nothing
                    break;
            }

            if (playerResult.Clutched)
            {
                switch (playerResult.ClutchKills)
                {
                    case Kill1:
                        matchStats.V1++;
                        break;
                    case Kill2:
                        matchStats.V2++;
                        break;
                    case Kill3:
                        matchStats.V3++;
                        break;
                    case Kill4:
                        matchStats.V4++;
                        break;
                    case Kill5:
                        matchStats.V5++;
                        break;
                    default:
                        // Do nothing
                        break;
                }
            }

            // TODO Kast
            // TODO ContributionScore

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
        _MatchCallback.StopDemoRecording();

        var delay = 15;

        if (_MatchCallback.GetConvar<bool>("tv_enable") || _MatchCallback.GetConvar<bool>("tv_enable1"))
        {
            // TV Delay in s
            var tvDelaySeconds = Math.Max(_MatchCallback.GetConvar<int>("tv_delay"), _MatchCallback.GetConvar<int>("tv_delay1"));
            _Logger.LogInformation("Waiting for sourceTV. Delay: {delay}s + 15s", tvDelaySeconds);
            delay += tvDelaySeconds;
        }

        await Task.Delay(TimeSpan.FromSeconds(delay)).ConfigureAwait(false);

        if (_DemoUploader != null)
        {
            await _DemoUploader.UploadDemoAsync(MatchInfo.DemoFile, CancellationToken.None).ConfigureAwait(false);
        }

        if (MatchInfo.MatchMaps[MatchInfo.MatchMaps.Count - 1] == MatchInfo.CurrentMap)
        {
            var seriesResultParams = new SeriesResultParams(MatchInfo.Config.MatchId, MatchInfo.MatchMaps.GroupBy(x => x.Winner).MaxBy(x => x.Count())!.Key!.TeamConfig.Name, Forfeit: true, 120000, MatchInfo.MatchMaps.Count(x => x.Team1Points > x.Team2Points), MatchInfo.MatchMaps.Count(x => x.Team2Points > x.Team1Points));
            await _ApiProvider.FinalizeAsync(seriesResultParams, CancellationToken.None).ConfigureAwait(false);
        }

        foreach (var player in AllMatchPlayers)
        {
            player.Player.Kick();
        }

        await TryFireStateAsync(MatchCommand.CleanUpMatch).ConfigureAwait(false);
    }

    private void CleanUpMatch()
    {
        _MatchCallback.CleanUpMatch();
    }

    private async Task MatchLiveAsync()
    {
        for (int i = 0; i < NumOfMatchLiveMessages; i++)
        {
            _MatchCallback.SendMessage(string.Create(CultureInfo.InvariantCulture, $" {ChatColors.Default}Match is {ChatColors.Green}LIVE"));

            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
        }
    }

    private void OnMatchStateChanged(StateMachine<MatchState, MatchCommand>.Transition transition)
    {
        _Logger.LogInformation("MatchState Changed: {source} => {destination}", transition.Source, transition.Destination);
    }

    private void SwitchToMatchMap()
    {
        _MatchCallback.SwitchMap(MatchInfo.CurrentMap.MapName);
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
        if (!_ReadyReminderTimer.Enabled)
        {
            return;
        }

        try
        {
            _Logger.LogInformation("ReadyReminder Elapsed");
            var readyPlayerIds = AllMatchPlayers.Where(p => p.IsReady).Select(x => x.Player.SteamID).ToList();
            var notReadyPlayers = _MatchCallback.GetAllPlayers().Where(p => !readyPlayerIds.Contains(p.SteamID));

            var remindMessage = $" {ChatColors.Default}You are {ChatColors.Error}not {ChatColors.Default}ready! Type {ChatColors.Command}!ready {ChatColors.Default}if you are ready.";
            foreach (var player in notReadyPlayers)
            {
                player.PrintToChat(remindMessage);
            }
        }
        catch (Exception ex)
        {
            _Logger.LogError(ex, "Error sending vote reminder");
        }
    }

    public string CreateDotGraph()
    {
        return UmlDotGraph.Format(_MatchStateMachine.GetInfo());
    }

    private void SendRemainingMapsToVotingTeam()
    {
        SwitchVotingTeam();

        _MapsToSelect.ForEach(m => m.Votes.Clear());

        var mapOptions = new List<MenuOption>();

        for (int i = 0; i < _MapsToSelect.Count; i++)
        {
            var mapNumber = i;
            string? map = _MapsToSelect[mapNumber].Name;
            mapOptions.Add(new MenuOption(map, (opt, player) => BanMap(player, mapNumber)));
        }

        ShowMenuToTeam(_CurrentMatchTeamToVote!, string.Create(CultureInfo.InvariantCulture, $" {ChatColors.Default}Vote to ban map: type {ChatColors.Command}!<mapnumber>"), mapOptions);

        _VoteTimer.Start();
    }

    private void RemoveBannedMap()
    {
        _VoteTimer.Stop();

        var mapToBan = _MapsToSelect.MaxBy(m => m.Votes.Count);
        _MapsToSelect.Remove(mapToBan!);
        _MapsToSelect.ForEach(x => x.Votes.Clear());

        _MatchCallback.SendMessage(string.Create(CultureInfo.InvariantCulture, $" {ChatColors.Default}Map {ChatColors.Highlight}{mapToBan!.Name} {ChatColors.Default}was banned by {_CurrentMatchTeamToVote?.TeamConfig.Name}!"));

        if (_MapsToSelect.Count == 1)
        {
            MatchInfo.CurrentMap.MapName = _MapsToSelect[0].Name;
            _MapsToSelect = MatchInfo.Config.Maplist.Select(x => new Vote(x)).ToList();
        }
    }

    private void SendTeamVoteToVotingteam()
    {
        SwitchVotingTeam();

        var mapOptions = new List<MenuOption>()
        {
            new("T", (opt, player) => VoteTeam(player, "T")),
            new("CT", (opt, player) => VoteTeam(player, "CT")),
        };

        ShowMenuToTeam(_CurrentMatchTeamToVote!, "Choose starting side:", mapOptions);

        _VoteTimer.Start();
    }

    private void SetSelectedTeamSite()
    {
        _VoteTimer.Stop();

        var startTeam = _TeamVotes.MaxBy(m => m.Votes.Count)!.Name.Equals("T", StringComparison.OrdinalIgnoreCase) ? Team.Terrorist : Team.CounterTerrorist;
        _Logger.LogInformation("Set selected teamsite to {startTeam}. Voted by {team}", startTeam, _CurrentMatchTeamToVote!.TeamConfig.Name);

        if (_CurrentMatchTeamToVote!.CurrentTeamSite != startTeam)
        {
            _CurrentMatchTeamToVote.StartingTeamSite = startTeam;
            _CurrentMatchTeamToVote.CurrentTeamSite = startTeam;
            var otherTeam = _CurrentMatchTeamToVote == MatchTeam1 ? MatchTeam2 : MatchTeam1;
            otherTeam.StartingTeamSite = startTeam == Team.Terrorist ? Team.CounterTerrorist : Team.Terrorist;
            otherTeam.CurrentTeamSite = otherTeam.StartingTeamSite;

            _Logger.LogInformation("{team} starts as Team {startTeam}", _CurrentMatchTeamToVote.TeamConfig.Name, _CurrentMatchTeamToVote!.CurrentTeamSite.ToString());
            _Logger.LogInformation("{team} starts as Team {startTeam}", otherTeam.TeamConfig.Name, otherTeam!.CurrentTeamSite.ToString());
        }

        _MatchCallback.SendMessage(string.Create(CultureInfo.InvariantCulture, $"{_CurrentMatchTeamToVote!.TeamConfig.Name} selected {startTeam} as startside!"));
    }

    private static void ShowMenuToTeam(MatchTeam team, string title, IEnumerable<MenuOption> options)
    {
        team.Players.ForEach(p =>
        {
            p.Player.ShowMenu(title, options);
        });
    }

    private void SwitchVotingTeam()
    {
        if (_CurrentMatchTeamToVote == null)
        {
            _CurrentMatchTeamToVote = MatchTeam1;
        }
        else
        {
            _CurrentMatchTeamToVote = _CurrentMatchTeamToVote == MatchTeam1 ? MatchTeam2 : MatchTeam1;
        }
    }

    private bool AllPlayersAreConnected()
    {
        var players = _MatchCallback.GetAllPlayers();
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
        var requiredPlayers = MatchInfo.Config.PlayersPerTeam * 2;

        _Logger.LogInformation("Match has {readyPlayers} of {rquiredPlayers} ready players: {readyPlayers}", readyPlayers.Count(), requiredPlayers, string.Join("; ", readyPlayers.Select(a => $"{a.Player.PlayerName}[{a.IsReady}]")));

        return readyPlayers.Take(requiredPlayers + 1).Count() == requiredPlayers;
    }

    private bool AllTeamsUnpaused() => !MatchTeam1.IsPaused && !MatchTeam2.IsPaused;

    private bool AllMapsArePlayed()
    {
        var teamWithMostWins = MatchInfo.MatchMaps.Where(x => x.Winner != null).GroupBy(x => x.Winner).MaxBy(x => x.Count());
        if (teamWithMostWins?.Key == null)
        {
            _Logger.LogError("Can not check if all maps are ready. No team with wins!");
            return false;
        }

        var wins = teamWithMostWins.Count();
        var requiredWins = MatchInfo.Config.NumMaps / 2d;
        _Logger.LogInformation("{team} has most wins: {wins} of {requiredWins}", teamWithMostWins.Key.TeamConfig.Name, wins, requiredWins);

        return wins > requiredWins;
    }

    private bool NotAllMapsArePlayed() => !AllMapsArePlayed();

    private void SetAllPlayersNotReady()
    {
        _Logger.LogInformation("Reset Readystate for all players");

        foreach (var player in AllMatchPlayers)
        {
            player.IsReady = false;
        }

        _MatchCallback.SendMessage($"Waiting for all players to be ready.");
        _MatchCallback.SendMessage(string.Create(CultureInfo.InvariantCulture, $" {ChatColors.Command}!ready {ChatColors.Default}to toggle your ready state."));
    }

    private bool MapIsSelected()
    {
        // The SelectedCount is checked when the Votes are done but the map is still in the list
        return _MapsToSelect.Count == 2;
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
        _Logger.LogInformation("GetMatchTeam for {steamId} in MatchTeam1: {team1Ids}", steamID, string.Join(", ", MatchTeam1.Players.Select(x => x.Player.SteamID)));
        if (MatchTeam1.Players.Exists(x => x.Player.SteamID.Equals(steamID)))
        {
            return MatchTeam1;
        }

        _Logger.LogInformation("GetMatchTeam for {steamId} in MatchTeam2: {team1Ids}", steamID, string.Join(", ", MatchTeam2.Players.Select(x => x.Player.SteamID)));
        if (MatchTeam2.Players.Exists(x => x.Player.SteamID.Equals(steamID)))
        {
            return MatchTeam2;
        }

        return null;
    }

    private MatchTeam? GetMatchTeam(Team team)
    {
        return MatchTeam1.CurrentTeamSite == team ? MatchTeam1 : MatchTeam2;
    }

    private MatchPlayer GetMatchPlayer(ulong steamID)
    {
        return AllMatchPlayers.First(x => x.Player.SteamID == steamID);
    }

    #region Match Functions

    public bool TryAddPlayer(IPlayer player)
    {
        var isTeam1 = MatchInfo.Config.Team1.Players.ContainsKey(player.SteamID);
        var isTeam2 = !isTeam1 && MatchInfo.Config.Team2.Players.ContainsKey(player.SteamID);
        if (!isTeam1 && !isTeam2)
        {
            _Logger.LogInformation("Player with steam id {steamId} is no member of this match!", player.SteamID);
            return false;
        }

        var team = isTeam1 ? MatchTeam1 : MatchTeam2;
        var startSite = team.CurrentTeamSite;
        if (startSite == Team.None)
        {
            startSite = isTeam1 ? Team.Terrorist : Team.CounterTerrorist;
        }

        _Logger.LogInformation("Player {playerName} belongs to {teamName}", player.PlayerName, team.TeamConfig.Name);

        if (player.Team != startSite)
        {
            _Logger.LogInformation("Player {playerName} should be on {startSite} but is {currentTeam}", player.PlayerName, startSite, player.Team);

            player.SwitchTeam(startSite);
        }

        var existingPlayer = team.Players.Find(x => x.Player.SteamID.Equals(player.SteamID));
        if (existingPlayer != null)
        {
            team.Players.Remove(existingPlayer);
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

        // TODO Error when Player was not ready
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

    public async Task TogglePlayerIsReadyAsync(IPlayer player)
    {
        if (CurrentState != MatchState.WaitingForPlayersConnectedReady && CurrentState != MatchState.WaitingForPlayersReady)
        {
            player.PrintToChat("Currently ready state is not awaited!");
            return;
        }

        var matchPlayer = GetMatchPlayer(player.SteamID);
        matchPlayer.IsReady = !matchPlayer.IsReady;

        var readyPlayers = MatchTeam1.Players.Count(x => x.IsReady) + MatchTeam2.Players.Count(x => x.IsReady);

        // Min Players per Team
        var requiredPlayers = MatchInfo.Config.MinPlayersToReady * 2;

        if (matchPlayer.IsReady)
        {
            _MatchCallback.SendMessage(string.Create(CultureInfo.InvariantCulture, $"{player.PlayerName} is ready! {readyPlayers} of {requiredPlayers} are ready."));
            await TryFireStateAsync(MatchCommand.PlayerReady).ConfigureAwait(false);
        }
        else
        {
            _MatchCallback.SendMessage(string.Create(CultureInfo.InvariantCulture, $"{player.PlayerName} is not ready! {readyPlayers} of {requiredPlayers} are ready."));
        }
    }

    public Team GetPlayerTeam(ulong steamID)
    {
        var matchTeam = GetMatchTeam(steamID);

        if (matchTeam != null)
        {
            return matchTeam.CurrentTeamSite;
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

        return Team.None;
    }

    public bool BanMap(IPlayer player, int mapNumber)
    {
        if (CurrentState != MatchState.MapVote)
        {
            player.PrintToChat("Currently no map vote is active!");
            return false;
        }

        if (_CurrentMatchTeamToVote == null)
        {
            player.PrintToChat("There is not current matchteam to vote!");
            return false;
        }

        if (!_CurrentMatchTeamToVote.Players.Select(x => x.Player.UserId).Contains(player.UserId))
        {
            player.PrintToChat("You are currently not permitted to ban a map!");
            return false;
        }


        if (_MapsToSelect.Count <= mapNumber || mapNumber < 0)
        {
            player.PrintToChat(string.Create(CultureInfo.InvariantCulture, $"Mapnumber {mapNumber} is not available!"));
            return false;
        }

        var bannedMap = _MapsToSelect.Find(x => x.Votes.Exists(x => x.UserId == player.UserId));
        if (bannedMap != null)
        {
            player.PrintToChat(string.Create(CultureInfo.InvariantCulture, $"You already banned mapnumber {_MapsToSelect.IndexOf(bannedMap)}: {bannedMap.Name} !"));
            return false;
        }

        var mapToSelect = _MapsToSelect[mapNumber];
        mapToSelect.Votes.Add(player);

        if (_MapsToSelect.Sum(x => x.Votes.Count) >= MatchInfo.Config.PlayersPerTeam)
        {
            TryFireState(MatchCommand.VoteMap);
        }

        return true;
    }

    public bool VoteTeam(IPlayer player, string teamName)
    {
        if (CurrentState != MatchState.TeamVote)
        {
            player.PrintToChat("Currently no team vote is active!");
            return false;
        }

        if (_CurrentMatchTeamToVote == null)
        {
            player.PrintToChat("There is not current matchteam to vote!");
            return false;
        }

        if (!_CurrentMatchTeamToVote.Players.Select(x => x.Player.UserId).Contains(player.UserId))
        {
            player.PrintToChat("You are currently not permitted to vote for a team!");
            return false;
        }

        var votedTeam = _TeamVotes.Find(x => x.Votes.Exists(x => x.UserId == player.UserId));
        if (votedTeam != null)
        {
            player.PrintToChat(string.Create(CultureInfo.InvariantCulture, $"You already voted for team {_MapsToSelect.IndexOf(votedTeam)}: {votedTeam.Name} !"));
            return false;
        }

        var teamToVote = _TeamVotes.Find(x => x.Name.Equals(teamName, StringComparison.OrdinalIgnoreCase));
        if (teamToVote == null)
        {
            player.PrintToChat(string.Create(CultureInfo.InvariantCulture, $"Team with name {teamName} is not available!"));
            return false;
        }

        teamToVote.Votes.Add(player);

        player.PrintToChat(string.Create(CultureInfo.InvariantCulture, $" {ChatColors.Default}You voted for {ChatColors.Highlight}{teamToVote.Name}"));

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
            player.PrintToChat("Pause is currently not possible!");
        }
    }

    public void Unpause(IPlayer player)
    {
        var team = GetMatchTeam(player.SteamID);
        if (team == null)
        {
            player.PrintToChat("Unpause is currently not possible!");
            return;
        }

        team.IsPaused = false;
        TryFireState(MatchCommand.Unpause);
    }

    public void SwitchTeam()
    {
        MatchTeam1.ToggleTeamSite();
        MatchTeam2.ToggleTeamSite();
    }



    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _VoteTimer.Elapsed -= VoteTimer_Elapsed;
                _VoteTimer.Dispose();

                _ReadyReminderTimer.Elapsed -= ReadyReminderTimer_Elapsed;
                _ReadyReminderTimer.Dispose();
            }

            disposedValue = true;
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

        _Logger.LogInformation("The winner is: {winner}", winnerTeam!.TeamConfig.Name);
        _ = TryFireStateAsync(MatchCommand.CompleteMap);
    }

    public bool PlayerBelongsToMatch(ulong steamId)
    {
        return MatchInfo.Config.Team1.Players.Any(x => x.Key.Equals(steamId))
                || MatchInfo.Config.Team2.Players.Any(x => x.Key.Equals(steamId));
    }

    #endregion
}
