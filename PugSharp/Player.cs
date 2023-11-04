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

        _UserId = playerController.UserId.Value;
        SteamID = playerController.SteamID;
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
            _Logger.LogInformation("_PlayerController for SteamId {steamId} is Invalid. Reload with {userId} ", SteamID, _UserId);
            _PlayerController = Utilities.GetPlayerFromUserid(_UserId);
            _Logger.LogInformation("_PlayerController for SteamId {steamId} reloaded. UserID: {userId}; PlayerName: {playerName}", SteamID, UserId, PlayerName);
        }
    }

    public ulong SteamID { get; }

    public int? UserId => NullIfInvalid(() => _PlayerController.UserId);

    public string PlayerName => DefaultIfInvalid(() => _PlayerController.PlayerName, string.Empty);

    public Team Team => DefaultIfInvalid(() => (Team)_PlayerController.TeamNum, Team.None);

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
