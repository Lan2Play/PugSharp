using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using Microsoft.Extensions.Logging;
using PugSharp.Logging;
using PugSharp.Match.Contract;
using System.Text.Json.Serialization;

namespace PugSharp;

public class Player : IPlayer
{
    private static readonly ILogger<Player> _Logger = LogManager.CreateLogger<Player>();
    private int _UserId;
    private CCSPlayerController _PlayerController;

    public Player(CCSPlayerController playerController)
    {
        if (!playerController.IsValid)
        {
            _Logger.LogError("PlayerController is invalid!");
        }

        _UserId = playerController.UserId!.Value;
        _PlayerController = playerController;
        if (_PlayerController.ActionTrackingServices != null)
        {
            MatchStats = new PlayerMatchStats(_PlayerController.ActionTrackingServices.MatchStats, this);
        }
    }

    private T DefaultIfInvalid<T>(Func<T> loadValue) where T : struct
    {
        ReloadPlayerController();
        return _PlayerController != null && _PlayerController.IsValid ? loadValue() : default;
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


    public ulong SteamID
    {
        get
        {
            return DefaultIfInvalid(() =>
            {
                var steamId = _PlayerController.SteamID;
                _Logger.LogInformation("SteamId for {playerName}: {steamId}", PlayerName, steamId);
                return steamId;
            });
        }
    }

    public int? UserId => NullIfInvalid(() => _PlayerController.UserId);

    public string PlayerName => DefaultIfInvalid(() => _PlayerController.PlayerName, string.Empty);

    public Team Team => (Team)_PlayerController.TeamNum;

    public IPlayerPawn PlayerPawn => new PlayerPawn(_PlayerController.PlayerPawn.Value);

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
                var money = _PlayerController.InGameMoneyServices?.Account;
                money = value;
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
            Server.NextFrame(() =>
            {
                _PlayerController.PlayerPawn.Value.CommitSuicide(true, true);
                MatchStats?.ResetStats();
            });
        }
    }

    public IPlayerMatchStats? MatchStats { get; }
}
