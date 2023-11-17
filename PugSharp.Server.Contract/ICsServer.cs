
using PugSharp.Match.Contract;

namespace PugSharp.Server.Contract;

public interface ICsServer
{
    string GameDirectory { get; }

    void DisableCheats();
    void EndWarmup();
    void ExecuteCommand(string v);
    T? GetConvar<T>(string name);
    bool IsMapValid(string selectedMap);
    IReadOnlyList<IPlayer> LoadAllPlayers();
    void LoadAndExecuteConfig(string configFileName);
    (int CtScore, int TScore) LoadTeamsScore();

    void NextFrame(Action value);
    void PauseMatch();
    void PrintToChatAll(string message);
    void RestartGame();
    void RestoreBackup(string roundBackupFile);
    void SetupRoundBackup(string prefix);
    string StartDemoRecording(string demoDirectory, string demoFileName);
    void StopDemoRecording();
    void SwitchMap(string selectedMap);
    void UnpauseMatch();
    void UpdateConvar<T>(string name, T value);
}
