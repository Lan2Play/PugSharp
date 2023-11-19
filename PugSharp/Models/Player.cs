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
            if (!_PlayerController.IsValid)
            {
                return null;
            }

            return _PlayerController.InGameMoneyServices?.Account;
        }

        set
        {
            if (_PlayerController.IsValid)
            {
#pragma warning disable S1854 // Unused assignments should be removed
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                var money = _PlayerController.InGameMoneyServices?.Account;
                money = value;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
#pragma warning restore S1854 // Unused assignments should be removed
            }
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
        if (_PlayerController.IsValid)
        {
            _PlayerController.SwitchTeam((CounterStrikeSharp.API.Modules.Utils.CsTeam)(int)team);
            CounterStrikeSharp.API.Server.NextFrame(() =>
            {
                _PlayerController.PlayerPawn.Value.CommitSuicide(explode: true, force: true);
                ResetScoreboard();
            });
        }
    }

    public void Kick()
    {
        CounterStrikeSharp.API.Server.ExecuteCommand(string.Create(CultureInfo.InvariantCulture, $"kickid {UserId!.Value} \"You are not part of the current match!\""));
    }

    private void ResetScoreboard()
    {
        var matchStats = _PlayerController.ActionTrackingServices?.MatchStats;

        if (matchStats != null)
        {
            matchStats.Kills = 0;
            matchStats.Deaths = 0;
        }
    }
}
