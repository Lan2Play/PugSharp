
namespace PugSharp.Match.Contract;

public interface IMatchCallback
{
    void EndWarmup();
    IReadOnlyList<IPlayer> GetAllPlayers();
    void PauseMatch();
    void SendMessage(string message);
    void SwitchMap(string selectedMap);
    void SwapTeams();
    void UnpauseMatch();

    void DisableCheats();
    string StartDemoRecording();
    void StopDemoRecording();
    void SetupRoundBackup();
    Team LoadMatchWinner();
    T? GetConvar<T>(string name);
    void CleanUpMatch();
    void RestoreBackup(string roundBackupFile);
}
