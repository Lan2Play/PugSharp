namespace PugSharp.G5Api;

public sealed class MatchPauseBeganEvent : MatchPauseEvent
{
    public MatchPauseBeganEvent(string matchId, int mapNumber, int teamNumber, PauseType pauseType) : base(matchId, mapNumber, teamNumber, pauseType, "pause_began")
    {
    }
}
