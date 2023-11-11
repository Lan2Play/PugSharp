namespace PugSharp.Api.G5Api;

public sealed class MatchUnpausedEvent : MatchPauseEvent
{
    public MatchUnpausedEvent() : base("game_unpaused")
    {
    }
}
