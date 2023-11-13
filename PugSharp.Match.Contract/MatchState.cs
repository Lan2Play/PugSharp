namespace PugSharp.Match.Contract;

public enum MatchState
{
    None,
    WaitingForPlayersConnectedReady,
    MapVote,
    TeamVote,
    SwitchMap,
    WaitingForPlayersReady,
    MatchStarting,
    MatchRunning,
    MatchPaused,
    MapCompleted,
    MatchCompleted,
    CleanUpMatch,
    RestoreMatch,
}
