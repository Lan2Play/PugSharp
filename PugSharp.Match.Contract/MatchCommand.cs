namespace PugSharp.Match.Contract;

public enum MatchCommand
{
    LoadMatch,
    ConnectPlayer,
    DisconnectPlayer,
    PlayerReady,
    VoteMap,
    VoteTeam,
    SwitchMap,
    StartMatch,
    StartKnifeRound,
    CompleteKnifeRound,
    StayAfterKnifeRound,
    SwitchAfterKnifeRound,
    CompleteMatch,
    CompleteMap,
    Pause,
    Unpause,
    TeamsDefined,
}
