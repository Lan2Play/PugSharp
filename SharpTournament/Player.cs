using CounterStrikeSharp.API.Core;
using SharpTournament.Match.Contract;
using System.Text.Json.Serialization;

namespace SharpTournament;

public class Player : IPlayer
{
    private readonly CCSPlayerController _PlayerController;

    public Player(CCSPlayerController playerController)
    {
        _PlayerController = playerController;
    }

    [JsonIgnore]
    public nint Handle => _PlayerController.Handle;

    public ulong SteamID => _PlayerController.SteamID;

    public int? UserId => _PlayerController.UserId;

    public string PlayerName => _PlayerController.PlayerName;

    public IPlayerPawn PlayerPawn => new PlayerPawn(_PlayerController.PlayerPawn.Value);

    public void PrintToChat(string message)
    {
        _PlayerController.PrintToChat(message);
    }

    public void SwitchTeam(Team team)
    {
        _PlayerController.SwitchTeam((CounterStrikeSharp.API.Modules.Utils.CsTeam)(int)team);
    }
}
