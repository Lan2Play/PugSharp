namespace SharpTournament.Match.Contract;

public interface IPlayer
{
    nint Handle { get; }

    ulong SteamID { get; }

    int? UserId { get; }

    IPlayerPawn PlayerPawn { get; }

    string PlayerName { get; }
    IPlayerMatchStats? MatchStats { get; }

    void PrintToChat(string message);
    void SwitchTeam(Team team);
}
