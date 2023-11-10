
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
    Task GoingLiveAsync(string matchId, string mapName, int mapNumber, CancellationToken cancellationToken);
    Task FinalizeMapAsync(string matchId, string winnerTeamName, int team1Score, int team2Score, int mapNumber, CancellationToken cancellationToken);
    Task SendRoundStatsUpdateAsync(string matchId, int mapNumber, ITeamInfo team1Info, ITeamInfo team2Info, IMap currentMap, CancellationToken cancellationToken);
    Task FinalizeAsync(string matchId, string winnerTeamName, bool forfeit, uint timeBeforeFreeingServerMs, int team1SeriesScore, int team2SeriesScore, CancellationToken cancellationToken);
}
