namespace PugSharp.Api.G5Api;

public sealed class RoundStartedEvent : RoundEvent
{
    public RoundStartedEvent(string matchId, int mapNumber, int roundNumber) : base(matchId, mapNumber, roundNumber, "round_start")
    {
    }
}
