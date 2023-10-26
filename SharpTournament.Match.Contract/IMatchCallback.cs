namespace SharpTournament.Match.Contract;

public interface IMatchCallback
{
    IReadOnlyList<IPlayer> GetAllPlayers();
    void SendMessage(string message);
    void SwitchTeam(IPlayer player, Team team);
}
