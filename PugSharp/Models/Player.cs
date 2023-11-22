using System.Globalization;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;

using PugSharp.Match.Contract;

namespace PugSharp.Models;

public class Player : IPlayer
{
    private readonly int _UserId;
    private CCSPlayerController _PlayerController;

    public Player(CCSPlayerController playerController)
    {
        _UserId = playerController.UserId!.Value;
        SteamID = playerController.SteamID;
        _PlayerController = playerController;

    }

    private T DefaultIfInvalid<T>(Func<T> loadValue, T defaultValue)
    {
        ReloadPlayerController();
        return _PlayerController != null && _PlayerController.IsValid ? loadValue() : defaultValue;
    }

    private T? NullIfInvalid<T>(Func<T?> loadValue)
    {
        ReloadPlayerController();
        return _PlayerController != null && _PlayerController.IsValid ? loadValue() : default;
    }

    private void ReloadPlayerController()
    {
        _PlayerController = Utilities.GetPlayers().Find(x => x.SteamID == SteamID) ?? _PlayerController;
        if (_PlayerController == null || !_PlayerController.IsValid)
        {
            _PlayerController = Utilities.GetPlayerFromUserid(_UserId);
        }
    }

    public ulong SteamID { get; }

    public int? UserId => NullIfInvalid(() => _PlayerController.UserId);

    public string PlayerName => DefaultIfInvalid(() => _PlayerController.PlayerName, string.Empty);

    public Team Team => DefaultIfInvalid(() => (Team)_PlayerController.TeamNum, Team.None);

    public int? Money
    {
        get
        {
            ReloadPlayerController();
            if (!_PlayerController.IsValid)
            {
                return null;
            }

            return _PlayerController.InGameMoneyServices?.Account;
        }

        set
        {
            ReloadPlayerController();
            if (_PlayerController.IsValid && _PlayerController.InGameMoneyServices != null && value != null)
            {
                _PlayerController.InGameMoneyServices.Account = value.Value;
            }
        }
    }

    public void PrintToChat(string message)
    {
        ReloadPlayerController();
        if (_PlayerController.IsValid)
        {
            _PlayerController.PrintToChat(message);
        }
    }

    public void ShowMenu(string title, IEnumerable<MenuOption> menuOptions)
    {
        ReloadPlayerController();
        var menu = new ChatMenu(title);

        foreach (var menuOption in menuOptions)
        {
            menu.AddMenuOption(menuOption.DisplayName, (player, opt) => menuOption.Action.Invoke(menuOption, this));
        }

        if (_PlayerController.IsValid)
        {
            ChatMenus.OpenMenu(_PlayerController, menu);
        }
    }

    public void SwitchTeam(Team team)
    {
        ReloadPlayerController();
        if (_PlayerController.IsValid)
        {
            _PlayerController.ChangeTeam((CounterStrikeSharp.API.Modules.Utils.CsTeam)(int)team);
        }
    }

    public void Kick()
    {
        CounterStrikeSharp.API.Server.ExecuteCommand(string.Create(CultureInfo.InvariantCulture, $"kickid {UserId!.Value} \"You are not part of the current match!\""));
    }
}
