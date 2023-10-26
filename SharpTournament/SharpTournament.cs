using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using SharpTournament.Config;
using Stateless;
using System.Text.Json;

namespace SharpTournament;

public enum MatchState
{
    None,
    WaitingForPlayersConnected,
    WaitingForPlayersConnectedReady,
    MapVote,
    TeamVote,
    SwitchMap,
    WaitingForPlayersReady,
    MatchStarting,
    MatchRunning,
    MatchPaused,
    MatchCompleted,
}

public enum MatchCommand
{
    LoadMatch,
    ConnectPlayer,
    DisconnectPlayer,
    PlayerReady,
    VoteMap,
    VoteTeam,
}

public class MatchPlayer
{
    public MatchPlayer(IPlayer player)
    {
        Player = player;
    }

    public IPlayer Player { get; }

    public bool IsReady { get; set; }
}

public class MatchTeam
{
    public MatchTeam(Team team)
    {
        Team = team;
    }

    public List<MatchPlayer> Players { get; } = new List<MatchPlayer>();
    public Team Team { get; }
}

public class Match
{
    private readonly IMatchCallback _MatchCallback;
    private readonly StateMachine<MatchState, MatchCommand> _MatchStateMachine;
    private readonly List<MatchTeam> _MatchTeams = new List<MatchTeam>();

    public Match(IMatchCallback matchCallback, MatchConfig matchConfig)
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
    public MatchConfig Config { get; }



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
        if (playerTeam == Team.None)
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

    public void TogglePlayerIsReady(Player player)
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

public interface IPlayer
{
    nint Handle { get; }

    ulong SteamID { get; }

    int? UserId { get; }

    IPlayerPawn PlayerPawn { get; }
}

public interface IPlayerPawn
{
    void CommitSuicide();
}

public class Player : IPlayer
{
    private readonly CCSPlayerController _PlayerController;

    public Player(CCSPlayerController playerController)
    {
        _PlayerController = playerController;
    }

    public nint Handle => _PlayerController.Handle;

    public ulong SteamID => _PlayerController.SteamID;

    public int? UserId => _PlayerController.UserId;

    public string PlayerName => _PlayerController.PlayerName;

    public IPlayerPawn PlayerPawn => new PlayerPawn(_PlayerController.PlayerPawn.Value);
}

public class PlayerPawn : IPlayerPawn
{
    private readonly CCSPlayerPawn _PlayerPawnHandle;

    public PlayerPawn(CCSPlayerPawn playerPawnHandle)
    {
        _PlayerPawnHandle = playerPawnHandle;
    }

    public void CommitSuicide()
    {
        _PlayerPawnHandle.CommitSuicide(true, true);
    }
}

public enum Team
{
    None,
    Team1 = 2,
    Team2 = 3,
}

public interface IMatchCallback
{
    IReadOnlyList<IPlayer> GetAllPlayers();
    void SendMessage(string message);
    void SwitchTeam(IPlayer player, Team team);
}

public class SharpTournament : BasePlugin, IMatchCallback
{
    public override string ModuleName => "SharpTournament Plugin";

    public override string ModuleVersion => "0.0.1";

    private readonly HttpClient _HttpClient = new();
    private Action<nint, int>? _SwitchTeamFunc;
    private Match? _Match;

    public override void Load(bool hotReload)
    {
        Console.WriteLine("Loading SharpTournament!");

        _SwitchTeamFunc = VirtualFunction.CreateVoid<IntPtr, int>(GameData.GetSignature("CCSPlayerController_SwitchTeam"));

    }

    public void InitializeMatch(MatchConfig matchConfig)
    {
        _Match = new Match(this, matchConfig);

        var players = GetAllPlayers();
        foreach (var player in players.Where(x => x.UserId.HasValue && x.UserId >= 0))
        {
            if (player.UserId != null && !_Match.TryAddPlayer(player))
            {
                KickPlayer(player.UserId.Value);
            }
        }
    }

    #region Commands

    [ConsoleCommand("st_loadconfig", "Load a match config")]
    public void OnCommandLoadConfig(CCSPlayerController? player, CommandInfo command)
    {
        Console.WriteLine("Start loading match config!");
        if (command.ArgCount != 3)
        {
            Console.WriteLine("Url is required as Argument!");
            player?.PrintToCenter("Url is required as Argument!");

            return;

        }

        var url = command.ArgByIndex(1);
        var authToken = command.ArgByIndex(2);
        LoadConfig(url, authToken);

        Console.WriteLine("Start Command called.");
    }

    [ConsoleCommand("st_dumpmatch", "Load a match config")]
    public void OnCommandDumpMatch(CCSPlayerController? player, CommandInfo command)
    {
        Console.WriteLine("################ dump match ################");
        Console.WriteLine(JsonSerializer.Serialize(_Match));
        Console.WriteLine("################ dump match ################");
    }

    [ConsoleCommand("ready", "Mark player as ready")]
    public void OnCommandReady(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
        {
            Console.WriteLine("Command Start has been called by the server. Player is required to be marked as ready");
            return;
        }

        _Match?.TogglePlayerIsReady(new Player(player));


        Console.WriteLine("Command ready called.");
    }

    [ConsoleCommand("st_start", "Starts a match")]
    public void OnCommandStart(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
        {
            Console.WriteLine("Command Start has been called by the server.");
            return;
        }

        Console.WriteLine("Start Command called.");
    }

    [ConsoleCommand("st_pause", "Pauses the current match")]
    public void OnCommandPause(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
        {
            Console.WriteLine("Command Pause has been called by the server.");
            return;
        }

        Console.WriteLine("Pause Command called.");
    }

    #endregion

    [GameEventHandler]
    public HookResult OnPlayerConnect(EventPlayerConnect @event, GameEventInfo info)
    {
        // // Userid will give you a reference to a CCSPlayerController class
        Console.WriteLine($"Player {@event.Userid.PlayerName} has connected!");

        if (_Match == null)
        {
            Console.WriteLine($"Player {@event.Userid.PlayerName} kicked because no match has been loaded!");
            Server.ExecuteCommand($"kickid {@event.Userid.UserId} \"No match loaded!\"");
            return HookResult.Continue;
        }
        else
        {
            @event.Userid.PrintToChat($"Hello {@event.Userid.PlayerName}, welcome to match {_Match.Config.MatchId}");
            if (!_Match.TryAddPlayer(new Player(@event.Userid)) && @event.Userid.UserId != null)
            {
                KickPlayer(@event.Userid.UserId.Value);
            }
        }
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
    {
        if (_Match != null)
        {
            var configTeam = _Match.GetPlayerTeam(@event.Userid.SteamID);

            if ((int)configTeam != @event.Team)
            {
                Console.WriteLine($"Player {@event.Userid.PlayerName} tried to join {@event.Team} but is not allowed!");
                var player = @event.Userid;
                var team = @event.Team;

                Server.NextFrame(() =>
                {

                    SwitchTeam(new Player(player), configTeam);
                    if (team == 1)
                    {
                        //TODO: player can cheat kills if switched to spectator
                        player.Score = 0;
                        //.m_pActionTrackingServices.Value.m_matchStats
                        // .Player.m_iKills = 0;
                    }
                });
                return HookResult.Continue;

            }
        }

        return HookResult.Continue;
    }




    public bool LoadConfig(string url, string authToken)
    {
        Console.WriteLine($"Loading match from \"{url}\"");

        try
        {
            _HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            var configJson = _HttpClient.GetStringAsync(url).Result;
            var config = JsonSerializer.Deserialize<MatchConfig>(configJson);
            if (config != null)
            {
                Console.WriteLine($"Successfully loaded config for match {config.MatchId}");
                InitializeMatch(config);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed loading config from \"{url}\". Error: {ex.Message};");
        }

        return false;
    }


    private static void KickPlayer(int userId)
    {
        Server.ExecuteCommand($"kickid {userId} \"You are not part of the current match!\"");
    }

    #region Implementation of IMatchCallback

    public void SwitchTeam(IPlayer player, Team team)
    {
        Console.WriteLine($"Switch player to team {team}");
        _SwitchTeamFunc?.Invoke(player.Handle, (int)team);
        player.PlayerPawn.CommitSuicide();
    }

    public IReadOnlyList<IPlayer> GetAllPlayers()
    {
        var playerEntities = Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller");
        return playerEntities.Select(p => new Player(p)).ToArray();
    }

    public void SendMessage(string message)
    {
        Server.PrintToChatAll(message);
    }

    #endregion


    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _HttpClient.Dispose();
        }
    }

}