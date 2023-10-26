using CounterStrikeSharp.API.Core;
using SharpTournament.Match.Contract;

namespace SharpTournament;

public class Player : IPlayer
{
    private readonly CCSPlayerController _PlayerController;

    public Player(CCSPlayerController playerController)
    {
        _PlayerController = playerController;
    }

    public nint Handle => _PlayerController.Handle;

    public ulong SteamID => _PlayerController.SteamID;

    public int? UserId => _PlayerController.UserId;

    public string PlayerName => _PlayerController.PlayerName;

    public IPlayerPawn PlayerPawn => new PlayerPawn(_PlayerController.PlayerPawn.Value);
}
