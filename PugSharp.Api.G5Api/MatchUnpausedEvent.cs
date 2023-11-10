namespace PugSharp.Api.G5Api;

public sealed class MatchUnpausedEvent : MatchPauseEvent
{
    public MatchUnpausedEvent(string matchId, int mapNumber, int teamNumber, PauseType pauseType) : base(matchId, mapNumber, teamNumber, pauseType, "game_unpaused")
    {
    }
}
