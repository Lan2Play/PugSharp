using SharpTournament.Match.Contract;
using Stateless;

namespace SharpTournament.Match;

public class Match
{
    private readonly IMatchCallback _MatchCallback;
    private readonly StateMachine<MatchState, MatchCommand> _MatchStateMachine;
    private readonly List<MatchTeam> _MatchTeams = new();

    public Match(IMatchCallback matchCallback, Config.MatchConfig matchConfig)
    {
        _MatchCallback = matchCallback;
        Config = matchConfig;

        _MatchStateMachine = new StateMachine<MatchState, MatchCommand>(MatchState.None);

        _MatchStateMachine.Configure(MatchState.None)
            .Permit(MatchCommand.LoadMatch, MatchState.WaitingForPlayersConnected);

        _MatchStateMachine.Configure(MatchState.WaitingForPlayersConnected)
            .PermitIf(MatchCommand.ConnectPlayer, MatchState.WaitingForPlayersConnectedReady, AllPlayersAreConnected);

        _MatchStateMachine.Configure(MatchState.WaitingForPlayersConnectedReady)
            .PermitIf(MatchCommand.PlayerReady, MatchState.MapVote, AllPlayersAreReady)
            .OnExit(SetAllPlayersNotReady);

        _MatchStateMachine.Configure(MatchState.MapVote)
            .PermitReentryIf(MatchCommand.VoteMap, () => { /*TODO Check if not all maps are vetoed*/ return true; })
            .PermitIf(MatchCommand.VoteMap, MatchState.TeamVote, () => { /*TODO Check if one map is selected*/ return true; })
            .OnEntry(() => /* TODO Send remaining map list to team X*/ { });

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

    public MatchState CurrentState => _MatchStateMachine.State;
    public Config.MatchConfig Config { get; }



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

    private void TryFireState(MatchCommand command)
    {
        if (_MatchStateMachine.CanFire(command))
        {
            _MatchStateMachine.Fire(command);
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

    private MatchTeam? GetMatchTeam(ulong steamID)
    {
        var team = GetPlayerTeam(steamID);
        return _MatchTeams.Find(x => x.Team == team);
    }

    private MatchPlayer GetMatchPlayer(ulong steamID)
    {
        return _MatchTeams.SelectMany(x => x.Players).First(x => x.Player.SteamID == steamID);
    }
}
