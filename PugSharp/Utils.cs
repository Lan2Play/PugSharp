using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using PugSharp.Config;
using PugSharp.Match;
using PugSharp.Match.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PugSharp
{
    public static class CounterStrikeSharpExtensions
    {
        internal static bool IsAdmin(this CCSPlayerController? playerController, ServerConfig? serverConfig)
        {
            if (serverConfig?.Admins == null || playerController == null)
            {
                return false;
            }

            return serverConfig.Admins.ContainsKey(playerController.SteamID);
        }

        internal static void Kick(this CCSPlayerController? playerController)
        {
            if (playerController?.UserId == null)
            {
                return;
            }

            Server.ExecuteCommand($"kickid {playerController.UserId.Value} \"You are not part of the current match!\"");
        }

        internal static void Kick(this IPlayer? player)
        {
            if (player?.UserId == null)
            {
                return;
            }

            Server.ExecuteCommand($"kickid {player.UserId.Value} \"You are not part of the current match!\"");
        }

        internal static PlayerConnectedState PlayerState(this CCSPlayerController player)
        {
            if (player == null || !player.IsValid)
            {
                return PlayerConnectedState.PlayerNeverConnected;
            }

            var statusRef = Schema.GetRef<UInt32>(player.Handle, "CBasePlayerController", "m_iConnected");

            return (PlayerConnectedState)statusRef;
        }
    }
}
