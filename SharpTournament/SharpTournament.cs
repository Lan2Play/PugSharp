using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory;
using System.Text.Json;

namespace SharpTournament;

public class SharpTournament : BasePlugin
{
    public override string ModuleName => "SharpTournament Plugin";

    public override string ModuleVersion => "0.0.1";

    private HttpClient _HttpClient = new();
    private Config? _Config;
    private Action<nint, int>? _SwitchTeamFunc;

    public override void Load(bool hotReload)
    {
        Console.WriteLine("Loading SharpTournament!");

        _SwitchTeamFunc = VirtualFunction.CreateVoid<IntPtr, int>(GameData.GetSignature("CCSPlayerController_SwitchTeam"));
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
            _Config = JsonSerializer.Deserialize<Config>(configJson);
            if (_Config != null)
            {
                Console.WriteLine($"Successfully loaded config for match {_Config.MatchId}");
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
        // Userid will give you a reference to a CCSPlayerController class
        Console.WriteLine($"Player {@event.Userid.PlayerName} has connected!");

        if (_Config == null)
        {
            Server.ExecuteCommand($"kickid {@event.Userid.UserId} \"No match loaded!\"");
            return HookResult.Continue;
        }

        if (_Config.Team1.Players.ContainsKey(@event.Userid.SteamID))
        {
            _SwitchTeamFunc?.Invoke(@event.Userid.Handle, 1);
        }
        if (_Config.Team2.Players.ContainsKey(@event.Userid.SteamID))
        {
            _SwitchTeamFunc?.Invoke(@event.Userid.Handle, 2);
        }
        else
        {
            Server.ExecuteCommand($"kickid {@event.Userid.UserId} \"You are not part of the current match!\"");
        }

        return HookResult.Continue;
    }
}