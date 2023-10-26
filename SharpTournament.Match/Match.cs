using SharpTournament.Match.Contract;
using Stateless;
using System.Text;

namespace SharpTournament.Match;

public class MapVote
{
    public MapVote(string mapName)
    {
        MapName = mapName;
    }

    public string MapName { get; }

    public List<IPlayer> Votes { get; } = new List<IPlayer>();
}

public class Match
{
    private readonly IMatchCallback _MatchCallback;
    private readonly StateMachine<MatchState, MatchCommand> _MatchStateMachine;
    private readonly List<MatchTeam> _MatchTeams = new();

    private List<MapVote> _MapsToSelect = new();
    private MatchTeam? _CurrentMatchTeamToVote;

    private System.Timers.Timer _VoteTimer = new();

    public Match(IMatchCallback matchCallback, Config.MatchConfig matchConfig)
    {
        _MatchCallback = matchCallback;
        Config = matchConfig;
        _VoteTimer.Interval = Config.VoteTimeout;
        _VoteTimer.Elapsed += _VoteTimer_Elapsed;

        var availbaleMaps = _MatchCallback.GetAvailableMaps();
        _MapsToSelect = availbaleMaps.Intersect(matchConfig.Maplist).Select(x => new MapVote(x)).ToList();


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
            .OnEntry(SendRemainingMapsToVotingTeam);

        _MatchStateMachine.Configure(MatchState.TeamVote)
            .PermitIf(MatchCommand.VoteTeam, MatchState.SwitchMap, () => { /*TODO Check if not all maps are vetoed*/ return true; });

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
        _MapsToSelect.ForEach(m => m.Votes.Clear());

        var mapMessageBuilder = new StringBuilder();

        mapMessageBuilder.AppendLine("Remaining maps to Veto: ");
        mapMessageBuilder.AppendLine();
        for (int i = 0; i < _MapsToSelect.Count; i++)
        {
            string? map = _MapsToSelect[i].MapName;
            mapMessageBuilder.Append(i).Append(": ").AppendLine(map);
        }

        mapMessageBuilder.AppendLine();
        mapMessageBuilder.AppendLine("To Veto: !veto [mapnumber] ");

        _CurrentMatchTeamToVote ??= _MatchTeams.First();

        var mapMessage = mapMessageBuilder.ToString();
        _CurrentMatchTeamToVote.Players.ForEach(p =>
        {
            p.Player.PrintToChat(mapMessage);
        });

        _CurrentMatchTeamToVote = GetMatchTeam(_CurrentMatchTeamToVote.Team == Team.Team1 ? Team.Team2 : Team.Team1);
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
        return _MapsToSelect.Count == 1;
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
            TryFireState(MatchCommand.VoteMap);
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

    public void SetVeto(IPlayer player, string mapNumber)
    {
        if (!_CurrentMatchTeamToVote.Players.Select(x => x.Player.UserId).Contains(player.UserId))
        {
            player.PrintToChat("You are currently not permitted to veto!");
            return;
        }

        if (!int.TryParse(mapNumber, out int mapNumberInt))
        {
            player.PrintToChat($"Mapnumber {mapNumber} is invalid!");
            return;
        }

        if (_MapsToSelect.Count <= mapNumberInt || mapNumberInt < 0)
        {
            player.PrintToChat($"Mapnumber {mapNumber} is not available!");
            return;
        }

        var vetoedMap = _MapsToSelect.FirstOrDefault(x => x.Votes.Any(x => x.UserId == player.UserId));
        if (vetoedMap != null)
        {
            player.PrintToChat($"You already vetoed Mapnumber {_MapsToSelect.IndexOf(vetoedMap)}: {vetoedMap.MapName} !");
            return;
        }

        var mapToSelect = _MapsToSelect[mapNumberInt];
        mapToSelect.Votes.Add(player);

        if (_MapsToSelect.Sum(x => x.Votes.Count) >= Config.PlayersPerTeam)
        {
            TryFireState(MatchCommand.VoteMap);
        }
    }

    #endregion

}
