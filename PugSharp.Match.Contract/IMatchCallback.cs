
namespace PugSharp.Match.Contract;

public interface IMatchCallback
{
    IReadOnlyList<IPlayer> GetAllPlayers();
    void PauseMatch();
    void SendMessage(string message);
    void SwitchMap(string selectedMap);
    void SwapTeams();
    void UnpauseMatch();
    string StartDemoRecording();
    void StopDemoRecording();
    void SetupRoundBackup();
    Team LoadMatchWinner();
    T? GetConvar<T>(string name);
    void CleanUpMatch();
    void RestoreBackup(string roundBackupFile);
    void StartWarmup();
    void StartingMatch();
}
