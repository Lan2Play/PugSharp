namespace PugSharp.Match.Contract;

public enum MatchState
{
    None,
    WaitingForPlayersConnectedReady,
    PreMapVote,
    MapVote,
    TeamVote,
    SwitchMap,
    WaitingForPlayersReady,
    MatchStarting,
    MatchRunning,
    MatchPaused,
    MapCompleted,
    MatchCompleted,
    RestoreMatch,
}
