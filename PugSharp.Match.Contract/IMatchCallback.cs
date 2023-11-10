
using PugSharp.Api.Contract;

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
    string StartDemoRecording();
    void StopDemoRecording();
    void SetupRoundBackup();
    Team LoadMatchWinner();
    T? GetConvar<T>(string name);
    Task GoingLiveAsync(GoingLiveParams goingLiveParams, CancellationToken cancellationToken);
    Task FinalizeMapAsync(MapResultParams finalizeMapParams, CancellationToken cancellationToken);
    Task SendRoundStatsUpdateAsync(RoundStatusUpdateParams roundStatusUpdateParams, CancellationToken cancellationToken);
    Task FinalizeAsync(SeriesResultParams seriesResultParams, CancellationToken cancellationToken);
    void CleanUpMatch();
}
