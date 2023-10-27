using SharpTournament.Match.Contract;
using Stateless;
using System.Text;

namespace SharpTournament.Match;

public class Vote
{
    public Vote(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public List<IPlayer> Votes { get; } = new List<IPlayer>();
}

public class MatchInfo
{
    public string SelectedMap { get; set; }
    public string StartTeam1 { get; set; }
    public string StartTeam2 { get; set; }
}

public class Match
{
    private readonly System.Timers.Timer _VoteTimer = new();
    private readonly IMatchCallback _MatchCallback;
    private readonly StateMachine<MatchState, MatchCommand> _MatchStateMachine;
    private readonly List<MatchTeam> _MatchTeams = new();

    private readonly List<Vote> _MapsToSelect;
    private readonly List<Vote> _TeamVotes = new() { new("T"), new("CT") };

    private readonly MatchInfo _MatchInfo = new();

    private MatchTeam? _CurrentMatchTeamToVote;


    public Match(IMatchCallback matchCallback, Config.MatchConfig matchConfig)
    {
        _MatchCallback = matchCallback;
        Config = matchConfig;
        _VoteTimer.Interval = Config.VoteTimeout;
        _VoteTimer.Elapsed += _VoteTimer_Elapsed;

        _MapsToSelect = matchConfig.Maplist.Select(x => new Vote(x)).ToList();

        _MatchStateMachine = new StateMachine<MatchState, MatchCommand>(MatchState.None);

        _MatchStateMachine.Configure(MatchState.None)
            .Permit(MatchCommand.LoadMatch, MatchState.WaitingForPlayersConnected);

        _MatchStateMachine.Configure(MatchState.WaitingForPlayersConnected)
            .PermitIf(MatchCommand.ConnectPlayer, MatchState.WaitingForPlayersConnectedReady, AllPlayersAreConnected);

        _MatchStateMachine.Configure(MatchState.WaitingForPlayersConnectedReady)
            .PermitIf(MatchCommand.PlayerReady, MatchState.MapVote, AllPlayersAreReady)
            .OnExit(SetAllPlayersNotReady);

        _MatchStateMachine.Configure(MatchState.MapVote)
            .PermitReentryIf(MatchCommand.VoteMap, MapIsNotSelected)
            .PermitIf(MatchCommand.VoteMap, MatchState.TeamVote, MapIsSelected)
            .OnEntry(SendRemainingMapsToVotingTeam)
            .OnExit(RemoveBannedMap);

        _MatchStateMachine.Configure(MatchState.TeamVote)
            .PermitIf(MatchCommand.VoteTeam, MatchState.SwitchMap, () => { /*TODO Check if team is selected*/ return true; })
            .OnEntry(SendTeamVoteToVotingteam)
            .OnExit(SetSelectedTeamSite);

        _MatchStateMachine.Configure(MatchState.SwitchMap);

        _MatchStateMachine.Configure(MatchState.WaitingForPlayersReady)
            .PermitIf(MatchCommand.PlayerReady, MatchState.MatchStarting, AllPlayersAreReady);

        _MatchStateMachine.Configure(MatchState.MatchStarting);

        _MatchStateMachine.Configure(MatchState.MatchRunning)
            .Permit(MatchCommand.DisconnectPlayer, MatchState.MatchPaused);

        _MatchStateMachine.Configure(MatchState.MatchPaused)
            .PermitIf(MatchCommand.ConnectPlayer, MatchState.MatchRunning, AllPlayersAreConnected);

        _MatchStateMachine.Configure(MatchState.MatchCompleted);

        _MatchStateMachine.Fire(MatchCommand.LoadMatch);

        //string graph = UmlDotGraph.Format(_MatchStateMachine.GetInfo());
    }

    private void _VoteTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        _VoteTimer.Stop();
        switch (CurrentState)
        {
            case MatchState.MapVote:
                TryFireState(MatchCommand.VoteMap);
                break;
        }
    }

    public MatchState CurrentState => _MatchStateMachine.State;

    public Config.MatchConfig Config { get; }



    private void SendRemainingMapsToVotingTeam()
    {
        SwitchVotingTeam();

        _MapsToSelect.ForEach(m => m.Votes.Clear());

        var mapMessageBuilder = new StringBuilder();

        mapMessageBuilder.AppendLine("Remaining maps to ban: ");
        mapMessageBuilder.AppendLine();
        for (int i = 0; i < _MapsToSelect.Count; i++)
        {
            string? map = _MapsToSelect[i].Name;
            mapMessageBuilder.Append(i).Append(": ").AppendLine(map);
        }

        mapMessageBuilder.AppendLine();
        mapMessageBuilder.AppendLine("To ban a map: !banmap [mapnumber] ");

        var mapMessage = mapMessageBuilder.ToString();
        SendMessageToTeam(_CurrentMatchTeamToVote!, mapMessage);
    }

    private void RemoveBannedMap()
    {
        var mapToBan = _MapsToSelect.MaxBy(m => m.Votes.Count);
        _MapsToSelect.Remove(mapToBan!);
        _MapsToSelect.ForEach(x => x.Votes.Clear());

        if (_MapsToSelect.Count == 1)
        {
            _MatchInfo.SelectedMap = _MapsToSelect[0].Name;
        }
    }

    private void SendTeamVoteToVotingteam()
    {
        SwitchVotingTeam();

        var teamMessageBuilder = new StringBuilder();

        teamMessageBuilder.AppendLine("Vote for your starting team");
        teamMessageBuilder.AppendLine();
        teamMessageBuilder.AppendLine("For T:  !voteteam T ");
        teamMessageBuilder.AppendLine("For CT: !voteteam CT ");


        var teamMessage = teamMessageBuilder.ToString();
        SendMessageToTeam(_CurrentMatchTeamToVote!, teamMessage);
    }

    private void SetSelectedTeamSite()
    {
        if (_CurrentMatchTeamToVote!.Team == Team.Team1)
        {
            _MatchInfo.StartTeam1 = _TeamVotes.MaxBy(m => m.Votes.Count)!.Name;
        }
        else
        {
            _MatchInfo.StartTeam1 = _TeamVotes.MinBy(m => m.Votes.Count)!.Name;
        }
    }


    private void SendMessageToTeam(MatchTeam team, string mapMessage)
    {
        team.Players.ForEach(p =>
        {
            p.Player.PrintToChat(mapMessage);
        });
    }

    private void SwitchVotingTeam()
    {
        if (_CurrentMatchTeamToVote == null)
        {
            _CurrentMatchTeamToVote = _MatchTeams.First();
        }
        else
        {
            _CurrentMatchTeamToVote = GetMatchTeam(_CurrentMatchTeamToVote.Team == Team.Team1 ? Team.Team2 : Team.Team1);
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
        return _MatchTeams.SelectMany(m => m.Players).All(p => p.IsReady);
    }

    private void SetAllPlayersNotReady()
    {
        foreach (var player in _MatchTeams.SelectMany(m => m.Players))
        {
            player.IsReady = false;
        }
    }

    private bool MapIsSelected()
    {
        // The SelectedCount is checked when the Votes are done but the map is still in the list
        return _MapsToSelect.Count == 1
            || (_MapsToSelect.Count == 2 && _MapsToSelect.SelectMany(x => x.Votes).Any());
    }

    private bool MapIsNotSelected()
    {
        return !MapIsSelected();
    }

    private void TryFireState(MatchCommand command)
    {
        if (_MatchStateMachine.CanFire(command))
        {
            _MatchStateMachine.Fire(command);
        }
    }

    private MatchTeam? GetMatchTeam(ulong steamID)
    {
        var team = GetPlayerTeam(steamID);
        return GetMatchTeam(team);
    }

    private MatchTeam? GetMatchTeam(Team team)
    {
        return _MatchTeams.Find(x => x.Team == team);
    }

    private MatchPlayer GetMatchPlayer(ulong steamID)
    {
        return _MatchTeams.SelectMany(x => x.Players).First(x => x.Player.SteamID == steamID);
    }

    #region Match Functions

    public bool TryAddPlayer(IPlayer player)
    {
        var playerTeam = GetPlayerTeam(player.SteamID);
        if (playerTeam == Contract.Team.None)
        {
            return false;
        }

        Console.WriteLine($"Player belongs to {playerTeam}");
        _MatchCallback.SwitchTeam(player, playerTeam);

        var team = _MatchTeams.Find(m => m.Team == playerTeam);
        if (team == null)
        {
            team = new MatchTeam(playerTeam);
            _MatchTeams.Add(team);
        }

        var existingPlayer = team.Players.Find(x => x.Player.SteamID.Equals(player.SteamID));
        if (existingPlayer != null)
        {
            team.Players.Remove(existingPlayer);
        }

        team.Players.Add(new MatchPlayer(player));

        TryFireState(MatchCommand.ConnectPlayer);
        return true;
    }

    public void TogglePlayerIsReady(IPlayer player)
    {
        var matchPlayer = GetMatchPlayer(player.SteamID);
        matchPlayer.IsReady = !matchPlayer.IsReady;

        var readyPlayers = _MatchTeams.SelectMany(x => x.Players).Count(x => x.IsReady);

        // Min Players per Team
        var requiredPlayers = Config.MinPlayersToReady * 2;

        if (matchPlayer.IsReady)
        {
            _MatchCallback.SendMessage($"\\x04{player.PlayerName} \\x06is ready! {readyPlayers} of {requiredPlayers} are ready.");
            TryFireState(MatchCommand.PlayerReady);
        }
        else
        {
            _MatchCallback.SendMessage($"\\x04{player.PlayerName} \\x02is not ready! {readyPlayers} of {requiredPlayers} are ready.");
        }
    }


    public Team GetPlayerTeam(ulong steamID)
    {
        if (Config.Team1.Players.ContainsKey(steamID))
        {
            return Team.Team1;
        }
        else if (Config.Team2.Players.ContainsKey(steamID))
        {
            return Team.Team2;
        }

        return Team.None;
    }

    public bool BanMap(IPlayer player, string mapNumber)
    {
        if (CurrentState != MatchState.MapVote)
        {
            player.PrintToChat("Currently no map vote is active!");
            return false;
        }

        if (!_CurrentMatchTeamToVote.Players.Select(x => x.Player.UserId).Contains(player.UserId))
        {
            player.PrintToChat("You are currently not permitted to ban a map!");
            return false;
        }

        if (!int.TryParse(mapNumber, out int mapNumberInt))
        {
            player.PrintToChat($"Mapnumber {mapNumber} is invalid!");
            return false;
        }

        if (_MapsToSelect.Count <= mapNumberInt || mapNumberInt < 0)
        {
            player.PrintToChat($"Mapnumber {mapNumber} is not available!");
            return false;
        }

        var bannedMap = _MapsToSelect.FirstOrDefault(x => x.Votes.Any(x => x.UserId == player.UserId));
        if (bannedMap != null)
        {
            player.PrintToChat($"You already banned mapnumber {_MapsToSelect.IndexOf(bannedMap)}: {bannedMap.Name} !");
            return false;
        }

        var mapToSelect = _MapsToSelect[mapNumberInt];
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

        if (!_CurrentMatchTeamToVote.Players.Select(x => x.Player.UserId).Contains(player.UserId))
        {
            player.PrintToChat("You are currently not permitted to vote for a team!");
            return false;
        }

        var votedTeam = _TeamVotes.Find(x => x.Votes.Any(x => x.UserId == player.UserId));
        if (votedTeam != null)
        {
            player.PrintToChat($"You already banned mapnumber {_MapsToSelect.IndexOf(votedTeam)}: {votedTeam.Name} !");
            return false;
        }

        var teamToVote = _TeamVotes.Find(x => x.Name.Equals(teamName, StringComparison.OrdinalIgnoreCase));
        if (teamToVote == null)
        {
            player.PrintToChat($"Team with name {teamName} is not available!");
            return false;
        }

        teamToVote.Votes.Add(player);

        if (_TeamVotes.Sum(x => x.Votes.Count) >= Config.PlayersPerTeam)
        {
            TryFireState(MatchCommand.VoteTeam);
        }

        return true;
    }

    #endregion

}
