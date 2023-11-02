using Microsoft.Extensions.Logging;
using PugSharp.Logging;
using PugSharp.Match.Contract;
using Stateless;
using Stateless.Graph;

namespace PugSharp.Match;

public class Match : IDisposable
{
    private static readonly ILogger<Match> _Logger = LogManager.CreateLogger<Match>();

    private readonly System.Timers.Timer _VoteTimer = new();
    private readonly System.Timers.Timer _ReadyReminderTimer = new(10000);
    private readonly IMatchCallback _MatchCallback;
    private readonly StateMachine<MatchState, MatchCommand> _MatchStateMachine;

    private readonly List<Vote> _MapsToSelect;
    private readonly List<Vote> _TeamVotes = new() { new("T"), new("CT") };

    private readonly MatchInfo _MatchInfo = new();

    private MatchTeam? _CurrentMatchTeamToVote;
    private bool disposedValue;

    public Match(IMatchCallback matchCallback, Config.MatchConfig matchConfig)
    {
        _MatchCallback = matchCallback;
        Config = matchConfig;
        _VoteTimer.Interval = Config.VoteTimeout;
        _VoteTimer.Elapsed += VoteTimer_Elapsed;
        _ReadyReminderTimer.Elapsed += ReadyReminderTimer_Elapsed;

        _MapsToSelect = matchConfig.Maplist.Select(x => new Vote(x)).ToList();

        _MatchStateMachine = new StateMachine<MatchState, MatchCommand>(MatchState.None);

        _MatchStateMachine.Configure(MatchState.None)
            .Permit(MatchCommand.LoadMatch, MatchState.WaitingForPlayersConnectedReady);

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
            .PermitIf(MatchCommand.CompleteMatch, MatchState.MatchCompleted, IsMatchReady)
            .OnEntryAsync(MatchLiveAsync);

        _MatchStateMachine.Configure(MatchState.MatchPaused)
            .PermitIf(MatchCommand.ConnectPlayer, MatchState.MatchRunning, AllPlayersAreConnected)
            .PermitIf(MatchCommand.Unpause, MatchState.MatchRunning, AllTeamsUnpaused)
            .OnEntry(PauseMatch)
            .OnExit(UnpauseMatch);

        _MatchStateMachine.Configure(MatchState.MatchCompleted)
            .OnEntry(CompleteMatch);


        _MatchStateMachine.OnTransitioned(OnMatchStateChanged);

        _MatchStateMachine.Fire(MatchCommand.LoadMatch);
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

    private bool IsMatchReady()
    {
        // TODO Check if one team has x rounds won or one team has given up?
        return true;
    }

    private void UnpauseMatch()
    {
        _MatchCallback.UnpauseServer();
    }

    private void PauseMatch()
    {
        MatchTeams.ForEach(team => { team.IsPaused = true; });
        _MatchCallback.PauseServer();
    }

    private void StartMatch()
    {
        foreach (var player in MatchTeams.SelectMany(x => x.Players).Where(x => x.Player.MatchStats != null))
        {
            player.Player.MatchStats!.ResetStats();
        }

        _MatchCallback.EndWarmup();
        _MatchCallback.DisableCheats();
        _MatchCallback.SetupRoundBackup();
        _MatchCallback.StartDemoRecording();

        TryFireState(MatchCommand.StartMatch);
    }

    private void CompleteMatch()
    {
        _MatchCallback.StopDemoRecording();
    }

    private async Task MatchLiveAsync()
    {
        for (int i = 0; i < 10; i++)
        {
            _MatchCallback.SendMessage($" {ChatColors.Default}Match is {ChatColors.Green}LIVE");

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    private void OnMatchStateChanged(StateMachine<MatchState, MatchCommand>.Transition transition)
    {
        _Logger.LogInformation("MatchState Changed: {source} => {destination}", transition.Source, transition.Destination);
    }

    private void SwitchToMatchMap()
    {
        _MatchCallback.SwitchMap(_MatchInfo.SelectedMap);
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
            var readyPlayerIds = MatchTeams.SelectMany(x => x.Players).Where(p => p.IsReady).Select(x => x.Player.SteamID).ToList();
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

    public MatchState CurrentState => _MatchStateMachine.State;

    public Config.MatchConfig Config { get; }

    public List<MatchTeam> MatchTeams { get; } = new();

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

        ShowMenuToTeam(_CurrentMatchTeamToVote!, $" {ChatColors.Default}Match is LIVEtype {ChatColors.Command}!<mapnumber>", mapOptions);

        _VoteTimer.Start();
    }

    private void RemoveBannedMap()
    {
        _VoteTimer.Stop();

        var mapToBan = _MapsToSelect.MaxBy(m => m.Votes.Count);
        _MapsToSelect.Remove(mapToBan!);
        _MapsToSelect.ForEach(x => x.Votes.Clear());

        _MatchCallback.SendMessage($" {ChatColors.Default}Map {ChatColors.Highlight}{mapToBan!.Name} {ChatColors.Default}was banned by {_CurrentMatchTeamToVote?.TeamConfig.Name}!");

        if (_MapsToSelect.Count == 1)
        {
            _MatchInfo.SelectedMap = _MapsToSelect[0].Name;
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

        if (_CurrentMatchTeamToVote!.Team == Team.Terrorist)
        {
            _MatchInfo.StartTeam1 = _TeamVotes.MaxBy(m => m.Votes.Count)!.Name;
            if(_MatchInfo.StartTeam1.Equals("CT", StringComparison.OrdinalIgnoreCase))
            {
                _MatchCallback.SwapTeams();
            }
        }
        else
        {
            _MatchInfo.StartTeam1 = _TeamVotes.MinBy(m => m.Votes.Count)!.Name;
            if (_MatchInfo.StartTeam1.Equals("T", StringComparison.OrdinalIgnoreCase))
            {
                _MatchCallback.SwapTeams();
            }
        }

        _MatchCallback.SendMessage($"{_CurrentMatchTeamToVote!.Team} selected {_MatchInfo.StartTeam1} as startside!");
    }

    private void ShowMenuToTeam(MatchTeam team, string title, IEnumerable<MenuOption> options)
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
            _CurrentMatchTeamToVote = MatchTeams[0];
        }
        else
        {
            _CurrentMatchTeamToVote = GetMatchTeam(_CurrentMatchTeamToVote.Team == Team.Terrorist ? Team.CounterTerrorist : Team.Terrorist);
        }
    }

    private bool AllPlayersAreConnected()
    {
        var players = _MatchCallback.GetAllPlayers();
        var connectedPlayerSteamIds = players.Select(p => p.SteamID).ToList();
        var allPlayerIds = Config.Team1.Players.Keys.Concat(Config.Team2.Players.Keys);
        if (allPlayerIds.All(p => connectedPlayerSteamIds.Contains(p)))
        {
            return true;
        }

        return false;
    }

    private bool AllPlayersAreReady()
    {
        var readyPlayers = MatchTeams.SelectMany(m => m.Players).Where(p => p.IsReady);
        var rquiredPlayers = Config.PlayersPerTeam * 2;

        _Logger.LogInformation($"Match has {readyPlayers.Count()} of {rquiredPlayers} ready players: {string.Join("; ", readyPlayers.Select(a => $"{a.Player.PlayerName}[{a.IsReady}]"))}");

        return readyPlayers.Count() == rquiredPlayers;
    }

    private bool AllTeamsUnpaused() => MatchTeams.TrueForAll(x => !x.IsPaused);

    private void SetAllPlayersNotReady()
    {
        _Logger.LogInformation("Reset Readystate for all players");

        foreach (var player in MatchTeams.SelectMany(m => m.Players))
        {
            player.IsReady = false;
        }

        _MatchCallback.SendMessage($"Waiting for all players to be ready.");
        _MatchCallback.SendMessage($" {ChatColors.Command}!ready {ChatColors.Default}to toggle your ready state.");
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

    private Task TryFireStateAsync(MatchCommand command)
    {
        if (_MatchStateMachine.CanFire(command))
        {
            return _MatchStateMachine.FireAsync(command);
        }

        return Task.CompletedTask;
    }

    private MatchTeam? GetMatchTeam(ulong steamID)
    {
        var team = GetPlayerTeam(steamID);
        return GetMatchTeam(team);
    }

    private MatchTeam? GetMatchTeam(Team team)
    {
        return MatchTeams.Find(x => x.Team == team);
    }

    private MatchPlayer GetMatchPlayer(ulong steamID)
    {
        return MatchTeams.SelectMany(x => x.Players).First(x => x.Player.SteamID == steamID);
    }

    #region Match Functions

    public bool TryAddPlayer(IPlayer player)
    {
        var playerTeam = GetPlayerTeam(player.SteamID);
        if (playerTeam == Team.None)
        {
            return false;
        }

        _Logger.LogInformation($"Player {player.PlayerName} belongs to {playerTeam}");

        if (player.Team != playerTeam)
        {
            player.SwitchTeam(playerTeam);
        }

        var team = MatchTeams.Find(m => m.Team == playerTeam);
        if (team == null)
        {
            team = new MatchTeam(playerTeam, playerTeam == Team.Terrorist ? Config.Team1 : Config.Team2);
            MatchTeams.Add(team);
        }

        var existingPlayer = team.Players.Find(x => x.Player.SteamID.Equals(player.SteamID));
        if (existingPlayer != null)
        {
            team.Players.Remove(existingPlayer);
        }

        team.Players.Add(new MatchPlayer(player));

        TryFireStateAsync(MatchCommand.ConnectPlayer);
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
            case MatchState.MatchRunning:
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

        var readyPlayers = MatchTeams.SelectMany(x => x.Players).Count(x => x.IsReady);

        // Min Players per Team
        var requiredPlayers = Config.MinPlayersToReady * 2;

        if (matchPlayer.IsReady)
        {
            _MatchCallback.SendMessage($"{player.PlayerName} is ready! {readyPlayers} of {requiredPlayers} are ready.");
            await TryFireStateAsync(MatchCommand.PlayerReady).ConfigureAwait(false);
        }
        else
        {
            _MatchCallback.SendMessage($"{player.PlayerName} is not ready! {readyPlayers} of {requiredPlayers} are ready.");
        }
    }

    public Team GetPlayerTeam(ulong steamID)
    {
        if (Config.Team1.Players.ContainsKey(steamID))
        {
            return Team.Terrorist;
        }
        else if (Config.Team2.Players.ContainsKey(steamID))
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
            player.PrintToChat($"Mapnumber {mapNumber} is not available!");
            return false;
        }

        var bannedMap = _MapsToSelect.Find(x => x.Votes.Exists(x => x.UserId == player.UserId));
        if (bannedMap != null)
        {
            player.PrintToChat($"You already banned mapnumber {_MapsToSelect.IndexOf(bannedMap)}: {bannedMap.Name} !");
            return false;
        }

        var mapToSelect = _MapsToSelect[mapNumber];
        mapToSelect.Votes.Add(player);

        if (_MapsToSelect.Sum(x => x.Votes.Count) >= Config.PlayersPerTeam)
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
            player.PrintToChat($"You already voted for team {_MapsToSelect.IndexOf(votedTeam)}: {votedTeam.Name} !");
            return false;
        }

        var teamToVote = _TeamVotes.Find(x => x.Name.Equals(teamName, StringComparison.OrdinalIgnoreCase));
        if (teamToVote == null)
        {
            player.PrintToChat($"Team with name {teamName} is not available!");
            return false;
        }

        teamToVote.Votes.Add(player);

        player.PrintToChat($" {ChatColors.Default}You voted for {ChatColors.Highlight}{teamToVote.Name}");

        if (_TeamVotes.Sum(x => x.Votes.Count) >= Config.PlayersPerTeam)
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


    #endregion

}
