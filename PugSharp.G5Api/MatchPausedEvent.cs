namespace PugSharp.G5Api;

public sealed class MatchPausedEvent : MatchPauseEvent
{
    public MatchPausedEvent(string matchId, int mapNumber, int teamNumber, PauseType pauseType) : base(matchId, mapNumber, teamNumber, pauseType, "game_paused")
    {
    }
}
