namespace PugSharp.Api.G5Api;

public sealed class MatchPausedEvent : MatchPauseEvent
{
    public MatchPausedEvent() : base("game_paused")
    {
    }
}
