namespace PugSharp.Match.Contract;

public interface IMatchCallback
{
    void EndWarmup();
    IReadOnlyList<IPlayer> GetAllPlayers();
    IReadOnlyCollection<string> GetAvailableMaps();
    void PauseMatch();
    void SendMessage(string message);
    void SwitchMap(string selectedMap);
    void SwapTeams();
    void UnpauseMatch();

    void DisableCheats();
    void StartDemoRecording();
    void StopDemoRecording();
    void SetupRoundBackup();
    Team LoadMatchWinnerName();
}
