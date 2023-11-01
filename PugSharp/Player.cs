using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using PugSharp.Match.Contract;
using System.Text.Json.Serialization;

namespace PugSharp;

public class Player : IPlayer
{
    private readonly CCSPlayerController _PlayerController;

    public Player(CCSPlayerController playerController)
    {
        _PlayerController = playerController;
        if (_PlayerController.IsValid && _PlayerController.ActionTrackingServices != null)
        {
            MatchStats = new PlayerMatchStats(_PlayerController.ActionTrackingServices.MatchStats, this);
        }
    }

    [JsonIgnore]
    public nint Handle => _PlayerController.Handle;

    public ulong SteamID => _PlayerController.SteamID;

    public int? UserId => _PlayerController.UserId;

    public string PlayerName => _PlayerController.PlayerName;

    public IPlayerPawn PlayerPawn => new PlayerPawn(_PlayerController.PlayerPawn.Value);

    public int? Money
    {
        get
        {
            return _PlayerController.InGameMoneyServices?.Account;
        }

        set
        {

            var money = _PlayerController.InGameMoneyServices?.Account;
            money = value;
        }
    }

    public void PrintToChat(string message)
    {
        _PlayerController.PrintToChat(message);
    }

    public void ShowMenu(string title, IEnumerable<MenuOption> menuOptions)
    {
        var menu = new ChatMenu(title);

        foreach (var menuOption in menuOptions)
        {
            menu.AddMenuOption(menuOption.DisplayName, (player, opt) => menuOption.Action.Invoke(menuOption, this));
        }

        ChatMenus.OpenMenu(_PlayerController, menu);
    }

    public void SwitchTeam(Team team)
    {
        _PlayerController.SwitchTeam((CounterStrikeSharp.API.Modules.Utils.CsTeam)(int)team);
    }

    public IPlayerMatchStats? MatchStats { get; }
}
