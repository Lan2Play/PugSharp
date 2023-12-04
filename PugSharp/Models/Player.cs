using System.Globalization;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;

using PugSharp.Match.Contract;

namespace PugSharp.Models;


//public class SchemaString<SchemaClass> : NativeObject where SchemaClass : NativeObject
//{
//    public SchemaString(SchemaClass instance, string member) : base(Schema.GetSchemaValue<nint>(instance.Handle, typeof(SchemaClass).Name!, member))
//    { }

//    public unsafe void Set(string str)
//    {
//        byte[] bytes = this.GetStringBytes(str);

//        for (int i = 0; i < bytes.Length; i++)
//        {
//            Unsafe.Write((void*)(this.Handle.ToInt64() + i), bytes[i]);
//        }

//        Unsafe.Write((void*)(this.Handle.ToInt64() + bytes.Length), 0);
//    }

//    private byte[] GetStringBytes(string str)
//    {
//        return Encoding.ASCII.GetBytes(str);
//    }
//}

public class Player : IPlayer
{
    public Player(ulong steamId)
    {
        SteamID = steamId;

    }

    private T DefaultIfInvalid<T>(Func<CCSPlayerController, T> loadValue, T defaultValue)
    {
        if (TryGetPlayerController(out var playerController))
        {
            return playerController != null && playerController.IsValid ? loadValue(playerController) : defaultValue;
        }

        return defaultValue;
    }

    private T? NullIfInvalid<T>(Func<CCSPlayerController, T?> loadValue)
    {
        if (TryGetPlayerController(out var playerController))
        {
            return playerController != null && playerController.IsValid ? loadValue(playerController) : default;
        }

        return default;
    }

    /// <summary>
    /// Try to load a player controller
    /// </summary>
    /// <returns>true if playerController is found and valid else false</returns>
    private bool TryGetPlayerController(out CCSPlayerController? playerController)
    {
        try
        {
            playerController = Utilities.GetPlayerFromSteamId(SteamID);
            if (playerController != null && playerController.IsValid)
            {
                return true;
            }
        }
        catch (Exception)
        {
            playerController = null;
        }

        return false;
    }

    public ulong SteamID { get; }

    public int? UserId => NullIfInvalid(p => p.UserId);

    public string PlayerName
    {
        get => DefaultIfInvalid(p => p.PlayerName, string.Empty);
        set
        {
            // Do nothing
        }
        //set
        //{
        //    if (TryGetPlayerController(out var playerController) && playerController?.InGameMoneyServices != null && value != null)
        //    {
        //        var playerName = new SchemaString<CBasePlayerController>(playerController, "m_iszPlayerName");
        //        playerName.Set(value);
        //    }
        //}
    }

    public Team Team => DefaultIfInvalid(p => (Team)p.TeamNum, Team.None);

    public int? Money
    {
        get
        {
            if (TryGetPlayerController(out var playerController))
            {
                return playerController?.InGameMoneyServices?.Account;
            }

            return null;
        }

        set
        {
            if (TryGetPlayerController(out var playerController) && playerController?.InGameMoneyServices != null && value != null)
            {
                playerController.InGameMoneyServices.Account = value.Value;
            }
        }
    }

    public string? Clan
    {
        get
        {
            if (TryGetPlayerController(out var playerController))
            {
                return playerController?.Clan;
            }

            return null;
        }

        set
        {
            if (TryGetPlayerController(out var playerController) && playerController?.InGameMoneyServices != null && value != null)
            {
                playerController.Clan = value;
            }
        }
    }

    public void PrintToChat(string message)
    {
        if (TryGetPlayerController(out var playerController))
        {
            playerController!.PrintToChat(message);
        }
    }

    public void ShowMenu(string title, IEnumerable<MenuOption> menuOptions)
    {
        if (TryGetPlayerController(out var playerController))
        {
            var menu = new ChatMenu(title);

            foreach (var menuOption in menuOptions)
            {
                menu.AddMenuOption(menuOption.DisplayName, (player, opt) => menuOption.Action.Invoke(menuOption, this));
            }

            ChatMenus.OpenMenu(playerController!, menu);
        }
    }

    public void SwitchTeam(Team team)
    {
        if (TryGetPlayerController(out var playerController))
        {
            playerController!.ChangeTeam((CounterStrikeSharp.API.Modules.Utils.CsTeam)(int)team);

        }
    }

    public void Kick()
    {
        try
        {
            if (TryGetPlayerController(out var playerController) && playerController!.Connected == PlayerConnectedState.PlayerConnected)
            {
                var userId = UserId;
                if (userId != null)
                {
                    CounterStrikeSharp.API.Server.ExecuteCommand(string.Create(CultureInfo.InvariantCulture, $"kickid {userId.Value} \"You are not part of the current match!\""));
                }
            }
        }
        catch (Exception)
        {
            // TODO Logging?
        }
    }
}
