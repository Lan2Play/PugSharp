using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory;
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
    ConnectPlayerReady,
    VoteMap,
}

public class Match
{
    private readonly IMatchCallback _MatchCallback;
    private readonly MatchConfig _Config;
    private readonly StateMachine<MatchState, MatchCommand> _MatchStateMachine;

    public Match(IMatchCallback matchCallback, MatchConfig matchConfig)
    {
        _MatchCallback = matchCallback;
        _Config = matchConfig;

        _MatchStateMachine = new StateMachine<MatchState, MatchCommand>(MatchState.None);

        _MatchStateMachine.Configure(MatchState.None)
            .Permit(MatchCommand.LoadMatch, MatchState.WaitingForPlayersConnected);

        _MatchStateMachine.Configure(MatchState.WaitingForPlayersConnected)
            .PermitIf(MatchCommand.ConnectPlayer, MatchState.WaitingForPlayersReady, AllPlayersAreConnected);

        _MatchStateMachine.Configure(MatchState.WaitingForPlayersConnectedReady)
            .PermitIf(MatchCommand.ConnectPlayerReady, MatchState.MapVote, () => { /*TODO Check if all Players are ready*/ return true; });

        _MatchStateMachine.Configure(MatchState.MapVote)
            .PermitReentryIf(MatchCommand.VoteMap, () => { /*TODO Check if not all maps are vetoed*/ return true; })
            .PermitIf(MatchCommand.VoteMap, MatchState.TeamVote, () => { /*TODO Check if one map is selected*/ return true; });


        _MatchStateMachine.Fire(MatchCommand.LoadMatch);
    }

    public MatchState CurrentState => _MatchStateMachine.State;

    private bool AllPlayersAreConnected()
    {
        var players = _MatchCallback.GetAllPlayers();
        var connectedPlayerSteamIds = players.Select(p => p.SteamID).ToList();
        var allPlayerIds = _Config.Team1.Players.Keys.Concat(_Config.Team2.Players.Keys);
        if (allPlayerIds.All(p => connectedPlayerSteamIds.Contains(p)))
        {
            return true;
        }

        return false;
    }

    public bool TryAddPlayer(IPlayer player)
    {
        if (_Config.Team1.Players.ContainsKey(player.SteamID))
        {
            Console.WriteLine("Player belongs to team1");
            _MatchCallback.SwitchTeam(player, Team.Team1);

        }
        else if (_Config.Team2.Players.ContainsKey(player.SteamID))
        {
            Console.WriteLine("Player belongs to team2");
            _MatchCallback.SwitchTeam(player, Team.Team2);
        }
        else
        {
            return false;

        }

        TryFireState(MatchCommand.ConnectPlayer);
        return true;
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
}

public interface IPlayer
{
    nint Handle { get; }
    ulong SteamID { get; }
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
}

public enum Team
{
    Team1 = 2,
    Team2 = 3,
}

public interface IMatchCallback
{
    IReadOnlyList<IPlayer> GetAllPlayers();

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
    }

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
            //Console.WriteLine("configdebug");
            //Console.WriteLine(JsonSerializer.Serialize(_Config));
            if (!_Match.TryAddPlayer(new Player(@event.Userid)))
            {
                Server.ExecuteCommand($"kickid {@event.Userid.UserId} \"You are not part of the current match!\"");

            }
            //if (_Config.Team1.Players.ContainsKey(@event.Userid.SteamID))
            //{
            //    Console.WriteLine("Player belongs to team1");
            //    _SwitchTeamFunc?.Invoke(@event.Userid.Handle, 2);
            //    return HookResult.Continue;

            //}
            //if (_Config.Team2.Players.ContainsKey(@event.Userid.SteamID))
            //{
            //    Console.WriteLine("Player belongs to team2");
            //    _SwitchTeamFunc?.Invoke(@event.Userid.Handle, 3);
            //    return HookResult.Continue;
            //}
            //else
            //{
            //    Server.ExecuteCommand($"kickid {@event.Userid.UserId} \"You are not part of the current match!\"");
            //}
        }
        return HookResult.Continue;
    }

    #region Implementation of IMatchCallback

    public void SwitchTeam(IPlayer player, Team team)
    {
        Console.WriteLine($"Switch player to team {team}");
        _SwitchTeamFunc?.Invoke(player.Handle, (int)team);
    }

    public IReadOnlyList<IPlayer> GetAllPlayers()
    {
        var playerEntities = Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller");
        return playerEntities.Select(p => new Player(p)).ToArray();
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