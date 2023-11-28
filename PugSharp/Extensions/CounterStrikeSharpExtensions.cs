using System.Globalization;

using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Memory;

namespace PugSharp.Extensions;

public static class CounterStrikeSharpExtensions
{
    internal static bool IsAdmin(this CCSPlayerController? playerController)
    {
        return AdminManager.PlayerHasPermissions(playerController, "@pugsharp/matchadmin");
    }

    internal static void Kick(this CCSPlayerController? playerController)
    {
        if (playerController?.UserId == null)
        {
            return;
        }

        CounterStrikeSharp.API.Server.ExecuteCommand(string.Create(CultureInfo.InvariantCulture, $"kickid {playerController.UserId.Value} \"You are not part of the current match!\""));
    }

    internal static PlayerConnectedState PlayerState(this CCSPlayerController player)
    {
        if (player == null || !player.IsValid)
        {
            return PlayerConnectedState.PlayerNeverConnected;
        }

        var statusRef = Schema.GetRef<uint>(player.Handle, "CBasePlayerController", "m_iConnected");

        return (PlayerConnectedState)statusRef;
    }

    internal static bool IsUtility(string weapon)
    {
        switch (weapon)
        {
            case "flashbang":
            case "hegrenade":
            case "inferno":
            case "smoke":

                {
                    return true;
                }

            default:
                {
                    return false;
                }
        }
    }
}
