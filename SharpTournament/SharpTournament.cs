using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;

namespace SharpTournament;
public class SharpTournament : BasePlugin
{
    public override string ModuleName => "SharpTournament Plugin";

    public override string ModuleVersion => "0.0.1";

    private int _Counter = 0;

    public override void Load(bool hotReload)
    {
        Console.WriteLine("Hello World!");
    }

    [ConsoleCommand("st_loadconfig", "Load a match config")]
    public void OnCommandLoadConfig(CCSPlayerController? player, CommandInfo command)
    {
        Console.WriteLine("Start loading match config!");
        if (command.ArgCount != 1)
        {
            Console.WriteLine("Url is required as Argument!");
            if (player != null)
            {
                player.PrintToCenter("Url is required as Argument!");
            }

            return;
        }

        var url = command.ArgByIndex(0);
        Console.WriteLine($"Loading match from \"{url}\"");



        Console.WriteLine("Start Command called.");
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

        _Counter++;

        if (_Counter % 2 == 0)
        {
            Server.ExecuteCommand($"kickid {@event.Userid.UserId} \"You are not part of the current match!\"");
        }

        return HookResult.Continue;
    }
}