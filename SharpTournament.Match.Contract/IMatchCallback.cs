namespace SharpTournament.Match.Contract;

public interface IMatchCallback
{
    IReadOnlyList<IPlayer> GetAllPlayers();
    IReadOnlyCollection<string> GetAvailableMaps();
    void SendMessage(string message);
    void SwitchTeam(IPlayer player, Team team);
}
