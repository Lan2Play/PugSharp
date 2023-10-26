using CounterStrikeSharp.API.Core;
using SharpTournament.Match.Contract;

namespace SharpTournament;

public class PlayerPawn : IPlayerPawn
{
    private readonly CCSPlayerPawn _PlayerPawnHandle;

    public PlayerPawn(CCSPlayerPawn playerPawnHandle)
    {
        _PlayerPawnHandle = playerPawnHandle;
    }

    public void CommitSuicide()
    {
        _PlayerPawnHandle.CommitSuicide(true, true);
    }
}
