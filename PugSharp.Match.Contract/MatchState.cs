namespace PugSharp.Match.Contract;

public enum MatchState
{
    None,
    WaitingForPlayersConnected,
    WaitingForPlayersConnectedReady,
    MapVote,
    TeamVote,
    SwitchMap,
    WaitingForPlayersReady,
    MatchStarting,
    MatchRunning,
    MatchPaused,
    MatchCompleted,
}
