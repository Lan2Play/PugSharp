namespace SharpTournament.Match.Contract;

public interface IMatchCallback
{
    void EndWarmup();
    IReadOnlyList<IPlayer> GetAllPlayers();
    IReadOnlyCollection<string> GetAvailableMaps();
    void PauseServer();
    void SendMessage(string message);
    void SwitchMap(string selectedMap);
    void UnpauseServer();
}
